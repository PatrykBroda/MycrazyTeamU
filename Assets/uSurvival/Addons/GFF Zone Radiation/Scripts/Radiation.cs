using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    // inventory, attributes etc. can influence max
    public interface IRadiationDebuf
    {
        short GetRadiationMaxBonus(short baseRadiationMax);
        short GetRadiationCleansingBonus();
    }

    public class Radiation : Energy, IHealthBonus
    {
        [Header("Components")]
        public Combat combat;
        public PlayerEquipment equipment;

        public float defaultRadiationResistance = 0;
        public short baseRadiationMax = 100;
        public short baseRadiationCleansingPerTick = -1;

        [HideInInspector]public RadiationZone currentRadiationSource;

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        IRadiationDebuf[] _bonusComponents;
        IRadiationDebuf[] bonusComponents =>
            _bonusComponents ?? (_bonusComponents = GetComponents<IRadiationDebuf>());

        public override short max
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (IRadiationDebuf bonusComponent in bonusComponents)
                    bonus += bonusComponent.GetRadiationMaxBonus(baseRadiationMax);
                return (short)(baseRadiationMax + bonus);
            }
        }

        public override short recoveryPerTick
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (IRadiationDebuf bonusComponent in bonusComponents)
                    bonus += bonusComponent.GetRadiationCleansingBonus();

                return (short)(baseRadiationCleansingPerTick + bonus);
            }
        }

        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerEnter(Collider col)
        {
            // heat source?
            RadiationZone radiationSource = col.GetComponent<RadiationZone>();
            if (radiationSource != null)
            {
                // none yet?
                if (currentRadiationSource == null)
                {
                    currentRadiationSource = radiationSource;
                }
                // otherwise keep closest one
                else if (currentRadiationSource != radiationSource) // different one? otherwise don't bother with calculations
                {
                    float oldDistance = Vector3.Distance(transform.position, currentRadiationSource.transform.position);
                    float newDistance = Vector3.Distance(transform.position, radiationSource.transform.position);
                    if (newDistance < oldDistance)
                        currentRadiationSource = radiationSource;
                }
            }
        }

        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerExit(Collider col)
        {
            RadiationZone radiationSource = col.GetComponent<RadiationZone>();
            if (radiationSource != null)
            {
                if (currentRadiationSource != null && currentRadiationSource.transform == col.transform)
                    currentRadiationSource = null;
            }
        }

        short IHealthBonus.HealthBonus(short baseHealth)
        {
            return 0;
        }

        short IHealthBonus.HealthRecoveryBonus()
        {
            int maskIndex = equipment.GetEquipmentTypeIndex("Mask");
            if (maskIndex != -1 && equipment.slots[maskIndex].amount > 0 && equipment.slots[maskIndex].item.secondItemDurability > 0)
            {
                ItemSlot equipmentSlot = equipment.slots[maskIndex];
                equipmentSlot.item.secondItemDurability -= 1;
                equipment.slots[maskIndex] = equipmentSlot;
            }

            if (currentRadiationSource != null && combat.radiationResistance < 1)
            {
                return (short)(currentRadiationSource.decreaseHealth - (currentRadiationSource.decreaseHealth * combat.radiationResistance));
            }
            else return 0;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            combat = gameObject.GetComponent<Combat>();
            equipment = gameObject.GetComponent<PlayerEquipment>();
            gameObject.GetComponent<Player>().radiation = this;
        }
#endif
    }
}