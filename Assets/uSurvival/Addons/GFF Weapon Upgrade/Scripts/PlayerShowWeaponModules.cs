using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public class PlayerShowWeaponModules : MonoBehaviour
    {
        [Header("Components")]
        public Player player;
        public PlayerEquipment equipment;

        private void Update()
        {
            if (player != null)
            {
                ItemSlot slot = equipment.slots[equipment.selection];
                if (slot.amount > 0 && slot.item.data is RangedWeaponItem weapon)
                {
                    WeaponModulesEnable details = weapon.GetWeaponModules(equipment);
                    if (details != null)
                    {
                        if (details.silencer != null) details.silencer.SetActive(slot.item.modulesHash[0] != 0);

                        //scope
                        if (details.sights != null && details.sights.Length > 0 && slot.item.modulesHash[3] != 0)
                        {
                            if (ScriptableItem.dict.TryGetValue(slot.item.modulesHash[3], out ScriptableItem itemData) && itemData is ScriptableWeaponModule module)
                            {
                                for (int i = 0; i < details.sights.Length; i++)
                                {
                                    if (details.sights[i].name == module.name) details.sights[i].SetActive(true);
                                    else details.sights[i].SetActive(false);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < details.sights.Length; i++)
                                    details.sights[i].SetActive(false);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < details.sights.Length; i++)
                                details.sights[i].SetActive(false);
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            player = gameObject.GetComponent<Player>();
            equipment = gameObject.GetComponent<PlayerEquipment>();
        }
#endif
    }
}


