using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class PlayerEquipment
    {
        public KeyCode[] pocketHotKeys;
        public KeyCode[] constructionHotKeys;

        void OnDragAndDrop_InventorySlot_PocketSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? (just check equality, rest is done server sided)
            if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
                inventory.slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
            {
                CmdMergeInventoryEquip(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
            }
        }

        void OnDragAndDrop_InventorySlot_ConstructionSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? (just check equality, rest is done server sided)
            if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
                inventory.slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
            {
                CmdMergeInventoryEquip(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
            }
        }

        void OnDragAndDrop_PocketSlot_InventorySlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? (just check equality, rest is done server sided)
            if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
            {
                CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]);
            }
        }

        void OnDragAndDrop_ConstructionSlot_InventorySlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? (just check equality, rest is done server sided)
            if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
            {
                CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]);
            }
        }

        void OnDragAndDrop_PocketSlot_PocketSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? (just check equality, rest is done server sided)
            if (slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
                slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
            {
                CmdMergeEquipEquip(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                CmdSwapEquipmentEquipment(slotIndices[0], slotIndices[1]);
            }
        }

        [Command]
        public void CmdMergeEquipEquip(int fromIndex, int toIndex)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and in the equipment
            if (health.current > 0 &&
                0 <= fromIndex && toIndex < slots.Count &&
                0 <= toIndex && toIndex < slots.Count)
            {
                // both items have to be valid
                ItemSlot slotFrom = slots[fromIndex];
                ItemSlot slotTo = inventory.slots[toIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the lists
                        slots[fromIndex] = slotFrom;
                        inventory.slots[toIndex] = slotTo;
                    }
                }
            }
        }
    }
}