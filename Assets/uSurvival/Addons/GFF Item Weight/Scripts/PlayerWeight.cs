using UnityEngine;
using Mirror;
using uSurvival;

namespace GFFAddons
{
    // inventory, attributes etc. can influence max health
    public interface IWeightBonus
    {
        int GetWeightCurrent();
        int GetWeightBonus();
    }

    [DisallowMultipleComponent]
    public class PlayerWeight : NetworkBehaviour, IMoveSpeedBonus
    {
        [Header("Components")]
        public Player player;
        public PlayerMovement movement;

        [Header("Settings")]
        public int defaultWeight = 7000;
        public float weightOverload = 2;

        // current value
        // set & get: keep between min and max
        public int weightCurrent
        {
            get
            {
                int value = 0;
                foreach (IWeightBonus component in bonusComponents)
                {
                    value += component.GetWeightCurrent();
                }

                return value;
            }
        }

        // cache components that give a bonus (attributes, inventory, etc.)
        IWeightBonus[] bonusComponents;
        private void Awake()
        {
            bonusComponents = GetComponentsInChildren<IWeightBonus>();
        }

        public int weightMax
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                int bonus = 0;
                foreach (IWeightBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.GetWeightBonus();

                return (defaultWeight + bonus);
            }
        }

        public float Percent() => (float)weightCurrent / weightMax;

        public string CurrentWeightToString()
        {
            return string.Format("{0:f1}", ((float)weightCurrent / 1000)) + "Kg";
        }

        public string CurrentWeightMaxToString()
        {
            return string.Format("{0:f1}", ((float)weightMax / 1000)) + "Kg";
        }

        public float GetMoveSpeedBonus()
        {
            return 0;
        }

        public float GetSpeedIncreasePercentageBonus()
        {
            return 0;
        }

        public float GetSpeedDecreasePercentageBonus()
        {
            if (weightCurrent < weightMax) return 0;
            else
            {
                return weightCurrent / (weightMax * weightOverload); 
            }
        }

#if UNITY_EDITOR
        // validation //////////////////////////////////////////////////////////////
        protected override void OnValidate()
        {
            base.OnValidate();

            if (syncInterval == 0)
            {
                syncInterval = 0.1f;
            }

            if (!player) player = gameObject.GetComponent<Player>();
            if (!movement) movement = player.movement;
            if (player != null && player.weight == null) player.weight = this;

            // gm tool data should only ever be synced to owner!
            // observers should not know about it!
            if (syncMode != SyncMode.Owner)
            {
                syncMode = SyncMode.Owner;
                UnityEditor.Undo.RecordObject(this, name + " " + GetType() + " component syncMode changed to Owner.");
            }
        }
#endif
    }
}