using GFFAddons;
using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class PlayerInventory
    {
        [Header("Components")]
        public PlayerEquipment equipment;

        [Header("Item Durability Restoration")]
        public ScriptableItemAndAmount itemForDurabilityRestoration;
        [HideInInspector] public int indexRecoveredItem = -1;
        [HideInInspector] public int inventoryIndexDurabilityRestoration = -1;

        void OnDragAndDrop_InventorySlot_DurabilityRestoreSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            if (slots[slotIndices[0]].amount > 0 && (slotIndices[1] == 1 && slots[slotIndices[0]].item.data is ScriptableDurabilityRestorer) ||
               (slotIndices[1] == 0 && (slots[slotIndices[0]].item.data is EquipmentItem || slots[slotIndices[0]].item.data is WeaponItem)))
            {
                if (slotIndices[1] == 0 && (slots[slotIndices[0]].item.data is EquipmentItem || slots[slotIndices[0]].item.data is WeaponItem))
                {
                    indexRecoveredItem = slotIndices[0];
                    equipment.indexRecoveredItem = -1;
                }
                else if (slotIndices[1] == 1) inventoryIndexDurabilityRestoration = slotIndices[0];
            }
        }

        [Command]
        public void CmdDurabilityRestore(int fromIndex, int toIndex)
        {
            {
                // validate: make sure that the slots actually exist in the inventory
                // and that they are not equal
                if (player.health.current > 0 && 0 <= fromIndex && fromIndex < slots.Count && 0 <= toIndex && toIndex < slots.Count &&
                    slots[fromIndex].amount > 0 && (slots[fromIndex].item.data is ScriptableDurabilityRestorer restorer) &&
                    slots[toIndex].amount > 0 && (slots[toIndex].item.data is EquipmentItem || slots[toIndex].item.data is WeaponItem))
                {
                    ItemSlot toSlot = slots[toIndex];
                    toSlot.item.durability = (ushort)Mathf.Clamp((ushort)(toSlot.item.durability + (toSlot.item.maxDurability * restorer.durabilityRestorer)), 0, toSlot.item.maxDurability);
                    slots[toIndex] = toSlot;

                    ItemSlot fromSlot = slots[fromIndex];
                    fromSlot.amount -= 1;
                    slots[fromIndex] = fromSlot;
                }
            }
        }
    }

    public partial class PlayerEquipment
    {


        [HideInInspector] public int indexRecoveredItem = -1;

        void OnDragAndDrop_EquipmentSlot_DurabilityRestoreSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            if (slots[slotIndices[0]].amount > 0 && (slotIndices[1] == 1 && slots[slotIndices[0]].item.data is ScriptableDurabilityRestorer) ||
               (slotIndices[1] == 0 && (slots[slotIndices[0]].item.data is EquipmentItem || slots[slotIndices[0]].item.data is WeaponItem)))
            {
                if (slotIndices[1] == 0 && (slots[slotIndices[0]].item.data is EquipmentItem || slots[slotIndices[0]].item.data is WeaponItem))
                {
                    ((PlayerInventory)inventory).indexRecoveredItem = -1;
                    indexRecoveredItem = slotIndices[0];
                }
            }
        }

        [Command]
        public void CmdDurabilityRestore(int fromIndex, int toIndex)
        {
            {
                // validate: make sure that the slots actually exist in the inventory
                // and that they are not equal
                if (health.current > 0 && 0 <= fromIndex && fromIndex < ((PlayerInventory)inventory).slots.Count && 0 <= toIndex && toIndex < slots.Count &&
                    ((PlayerInventory)inventory).slots[fromIndex].amount > 0 && (((PlayerInventory)inventory).slots[fromIndex].item.data is ScriptableDurabilityRestorer restorer) &&
                    slots[toIndex].amount > 0 && (slots[toIndex].item.data is EquipmentItem || slots[toIndex].item.data is WeaponItem))
                {
                    ItemSlot toSlot = slots[toIndex];
                    toSlot.item.durability = (ushort)Mathf.Clamp((ushort)(toSlot.item.durability + (toSlot.item.maxDurability * restorer.durabilityRestorer)), 0, toSlot.item.maxDurability);
                    slots[toIndex] = toSlot;

                    ItemSlot fromSlot = ((PlayerInventory)inventory).slots[fromIndex];
                    fromSlot.amount -= 1;
                    ((PlayerInventory)inventory).slots[fromIndex] = fromSlot;
                }
            }
        }
    }

    public partial class EquipmentItem
    {
        public ushort GetGoodDurability(ushort durabilityCurrent)
        {
            if (durabilityCurrent <= 0) return 0;
            else if (durabilityCurrent == maxDurability) return addSlots;
            else
            {
                if ((maxDurability / addSlots) < (maxDurability - durabilityCurrent))
                {
                    return (ushort)(addSlots / (maxDurability / durabilityCurrent));
                }
                else return addSlots;
            }
        }
    }
}