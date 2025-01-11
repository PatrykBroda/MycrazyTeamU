using GFFAddons;
using Mirror;
using UnityEngine;

namespace GFFAddons
{
    public enum WeaponModuleType : byte { none, Barrel, Handguard, Butt, Sights, Magazine }
}

namespace uSurvival
{
    public partial struct Item
    {
        public int[] modulesHash;
    }

    public partial class RangedWeaponItem
    {
        [Header("GFF Weapon Upgrade by Modules")]
        public ScriptableWeaponModule[] barrelModules;
        public ScriptableWeaponModule[] handguardModules;
        public ScriptableWeaponModule[] buttModules;
        public ScriptableWeaponModule[] sightsModules;
        public ScriptableWeaponModule[] magazineModules;

        public bool CanEquipWeaponModule(ScriptableWeaponModule module)
        {
            if (module.moduleType == WeaponModuleType.Barrel)
            {
                for (int i = 0; i < barrelModules.Length; i++)
                {
                    if (barrelModules[i].Equals(module)) return true;
                }
            }
            else if (module.moduleType == WeaponModuleType.Handguard)
            {
                for (int i = 0; i < handguardModules.Length; i++)
                {
                    if (handguardModules[i].Equals(module)) return true;
                }
            }
            else if (module.moduleType == WeaponModuleType.Butt)
            {
                for (int i = 0; i < buttModules.Length; i++)
                {
                    if (buttModules[i].Equals(module)) return true;
                }
            }
            else if (module.moduleType == WeaponModuleType.Sights)
            {
                for (int i = 0; i < sightsModules.Length; i++)
                {
                    if (sightsModules[i].Equals(module)) return true;
                }
            }
            else if (module.moduleType == WeaponModuleType.Magazine)
            {
                for (int i = 0; i < magazineModules.Length; i++)
                {
                    if (magazineModules[i].Equals(module)) return true;
                }
            }

            return false;
        }

        public int CanEquipWeaponModuleInModuleSlot(ScriptableWeaponModule module, int index)
        {
            //Debug.Log("trye check " + index);
            if (index == 0)
            {
                for (int i = 0; i < barrelModules.Length; i++)
                {
                    if (barrelModules[i].Equals(module)) return i;
                }
            }
            else if (index == 1)
            {
                for (int i = 0; i < handguardModules.Length; i++)
                {
                    if (handguardModules[i].Equals(module)) return i;
                }
            }
            else if (index == 2)
            {
                for (int i = 0; i < buttModules.Length; i++)
                {
                    if (buttModules[i].Equals(module)) return i;
                }
            }
            else if (index == 3)
            {
                for (int i = 0; i < sightsModules.Length; i++)
                {
                    if (sightsModules[i].Equals(module)) return i;
                }
            }
            else if (index == 4)
            {
                for (int i = 0; i < magazineModules.Length; i++)
                {
                    if (magazineModules[i].Equals(module)) return i;
                }
            }

            return -1;
        }

        // helper functions ////////////////////////////////////////////////////////
        public WeaponModulesEnable GetWeaponModules(PlayerEquipment equipment)
        {
            if (equipment.weaponMount != null && equipment.weaponMount.childCount > 0)
                return equipment.weaponMount.GetChild(0).GetComponentInChildren<WeaponModulesEnable>();
            return null;
        }
    }

    public partial class Player
    {
        void OnDragAndDrop_InventorySlot_WeaponModuleSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
            CmdSwapModuleFromInventorySlotToWeapon(slotIndices[0], slotIndices[1]);
        }
        void OnDragAndDrop_WeaponModuleSlot_InventorySlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
            CmdSwapModuleFromWeaponToInventorySlot(slotIndices[0], slotIndices[1]);
        }
        void OnDragAndDrop_WeaponModuleSlot_Inventory(int[] slotIndices)
        {
            CmdSwapModuleFromWeaponToInventoryPanel(slotIndices[0], slotIndices[1]);
        }
        void OnDragAndDrop_WeaponModuleSlot_InventorySlotFrom(int[] slotIndices)
        {

        }

        [Command]
        public void CmdSwapModuleFromInventorySlotToWeapon(int fromIndex, int toIndex)
        {
            if (inventory.slots[fromIndex].amount > 0 && inventory.slots[fromIndex].item.data is ScriptableWeaponModule module)
            {
                int weaponIndex = GetWeaponIndex(toIndex);
                //int weaponIndex = toIndex;
                if (weaponIndex != -1 && equipment.slots[weaponIndex].amount > 0 && equipment.slots[weaponIndex].item.data is RangedWeaponItem rWeapon)
                {
                    if (rWeapon.CanEquipWeaponModule(module))
                    {
                        int moduleIndex = GetModuleIndex(toIndex);
                        if (moduleIndex != -1 && inventory.slots[fromIndex].item.data is ScriptableWeaponModule mod)
                        {
                            //swap
                            if (equipment.slots[weaponIndex].item.modulesHash[moduleIndex] != 0)
                            {
                                int hash = equipment.slots[weaponIndex].item.modulesHash[moduleIndex];
                                inventory.Add(new Item(ScriptableItem.dict[hash]), 1, false);
                            }

                            UpdateEquipmentModuleHash(weaponIndex, moduleIndex, mod.name.GetStableHashCode());

                            //clear inventory slot
                            inventory.slots[fromIndex] = new ItemSlot();
                        }
                    }
                }
            }
        }
        [Command]
        public void CmdSwapModuleFromWeaponToInventoryPanel(int fromIndex, int toIndex)
        {
            int weaponIndex = GetWeaponIndex(fromIndex);
            if (weaponIndex != -1 && equipment.slots[weaponIndex].amount > 0 && equipment.slots[weaponIndex].item.data is RangedWeaponItem rWeapon)
            {
                int moduleIndex = GetModuleIndex(fromIndex);
                int hash = equipment.slots[weaponIndex].item.modulesHash[GetModuleIndex(fromIndex)];
                if (ScriptableItem.dict.ContainsKey(hash) && ScriptableItem.dict[hash] is ScriptableWeaponModule module)
                {
                    if (inventory.Add(new Item(ScriptableItem.dict[hash]), 1, false))
                    {
                        UpdateEquipmentModuleHash(weaponIndex, moduleIndex, 0);
                    }
                }
            }
        }
        [Command]
        public void CmdSwapModuleFromWeaponToInventorySlot(int fromIndex, int toIndex)
        {
            int weaponIndex = GetWeaponIndex(fromIndex);
            if (weaponIndex != -1 && equipment.slots[weaponIndex].amount > 0 && equipment.slots[weaponIndex].item.data is RangedWeaponItem weapon &&
                (inventory.slots[toIndex].amount == 0 || (inventory.slots[toIndex].item.data is ScriptableWeaponModule module && weapon.CanEquipWeaponModule(module))))
            {
                int moduleIndex = GetModuleIndex(fromIndex);
                int hash = equipment.slots[weaponIndex].item.modulesHash[GetModuleIndex(fromIndex)];
                if (ScriptableItem.dict.ContainsKey(hash))
                {
                    inventory.slots[toIndex] = new ItemSlot(new Item(ScriptableItem.dict[hash]), 1);
                    UpdateEquipmentModuleHash(weaponIndex, moduleIndex, 0);
                }
            }
        }
        [Server]
        public void EquipModuleByClick(int inventoryIndex)
        {
            Debug.Log("try use module from inventory slot " + inventoryIndex);

            if (inventory.slots[inventoryIndex].item.data is ScriptableWeaponModule module)
            {
                int moduleIndex = GetModuleIndex(module.moduleType);

                //check free slots for module
                bool added = false;
                for (int i = 0; i < equipment.slots.Count; i++)
                {
                    if (equipment.slots[i].amount > 0 && equipment.slots[i].item.data is RangedWeaponItem weapon)
                    {
                        if (equipment.slots[i].item.modulesHash[moduleIndex] == 0 && weapon.CanEquipWeaponModule(module))
                        {
                            Debug.Log("try use module from inventory is done " + moduleIndex);
                            UpdateEquipmentModuleHash(i, moduleIndex, module.name.GetStableHashCode());
                            inventory.slots[inventoryIndex] = new ItemSlot();
                            added = true;
                            break;
                        }
                    }
                }

                if (added == false)
                {
                    for (int i = 0; i < equipment.slots.Count; i++)
                    {
                        if (equipment.slots[i].amount > 0 && equipment.slots[i].item.data is RangedWeaponItem weapon && weapon.CanEquipWeaponModule(module))
                        {
                            int hash = equipment.slots[i].item.modulesHash[moduleIndex];
                            if (ScriptableItem.dict.ContainsKey(hash))
                                inventory.Add(new Item(ScriptableItem.dict[hash]), 1, false);

                            UpdateEquipmentModuleHash(i, moduleIndex, module.name.GetStableHashCode());
                            inventory.slots[inventoryIndex] = new ItemSlot();
                            break;
                        }
                    }
                }
            }
        }

        public int GetModuleIndex(WeaponModuleType type)
        {
            if (type == WeaponModuleType.Barrel) return 0;
            else if (type == WeaponModuleType.Handguard) return 1;
            else if (type == WeaponModuleType.Butt) return 2;
            else if (type == WeaponModuleType.Sights) return 3;
            else if (type == WeaponModuleType.Magazine) return 4;

            return -1;
        }

        private int GetWeaponIndex(int index)
        {
            if (index < 10) return 0;
            else if (index >= 10 && index < 20) return 1;
            else if (index >= 20) return 2;

            return -1;
        }
        private int GetModuleIndex(int toIndex)
        {
            if (toIndex.ToString().EndsWith("0")) return 0;
            else if (toIndex.ToString().EndsWith("1")) return 1;
            else if (toIndex.ToString().EndsWith("2")) return 2;
            else if (toIndex.ToString().EndsWith("3")) return 3;
            else if (toIndex.ToString().EndsWith("4")) return 4;

            return -1;
        }

        [Server]
        private void UpdateEquipmentModuleHash(int equipmentIndex, int moduleIndex, int newHash)
        {
            ItemSlot slot = equipment.slots[equipmentIndex];

            int[] modArray = new int[slot.item.modulesHash.Length];
            for (int i = 0; i < modArray.Length; i++)
            {
                if (i != moduleIndex) modArray[i] = slot.item.modulesHash[i];
                else modArray[i] = newHash;
            }

            slot.item.modulesHash = modArray;
            equipment.slots[equipmentIndex] = slot;

            //equipment.RefreshLocations();
        }

        private void OnModuleChanged(int oldValue, int newValue)
        {
            // refresh locations to hide the selected model
            equipment.RefreshLocations();
        }
    }

    public partial class PlayerEquipment
    {
        public int[] FindWeaponForModule(ScriptableWeaponModule module)
        {
            int[] indexes = new int[2] { -1, -1 };
            int moduleIndex = player.GetModuleIndex(module.moduleType);

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].amount > 0 && slots[i].item.data is RangedWeaponItem weapon)
                {
                    if (slots[i].item.modulesHash[moduleIndex] == 0 && weapon.CanEquipWeaponModuleInModuleSlot(module, moduleIndex) != -1)
                    {
                        indexes[0] = i;
                        indexes[1] = moduleIndex;
                        return indexes;
                    };
                }
            }
            return indexes;
        }
    }
}