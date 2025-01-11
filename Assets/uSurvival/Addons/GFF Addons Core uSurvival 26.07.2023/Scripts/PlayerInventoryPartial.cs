using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class Inventory
    {
        public ushort Count(ScriptableItem item)
        {
            // count manually. Linq is HEAVY(!) on GC and performance
            ushort amount = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.data.Equals(item))
                    amount += slot.amount;
            return amount;
        }

        // helper function to remove 'n' items from the inventory
        public bool Remove(ScriptableItem item, ushort amount)
        {
            for (int i = 0; i < slots.Count; ++i)
            {
                ItemSlot slot = slots[i];
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slot.amount > 0 && slot.item.data.Equals(item))
                {
                    // take as many as possible
                    amount -= slot.DecreaseAmount(amount);
                    slots[i] = slot;

                    // are we done?
                    if (amount == 0) return true;
                }
            }

            // if we got here, then we didn't remove enough items
            return false;
        }

        public int FindFreeSlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].amount == 0) return i;
            }

            return -1;
        }
    }

    public partial class PlayerInventory
    {
        [Command]
        public void CmdUpdateInventoryItemSlot(ItemSlot item, int index)
        {
            slots[index] = item;
        }

        [Command]
        public void CmdAddSlotToInventory(ItemSlot itemSlot)
        {
            bool isDone = false;

            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing stack afterwards)
            for (int i = 0; i < slots.Count; ++i)
            {
                // not empty and same type? then add free amount (max-amount)
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slots[i].amount > 0 && slots[i].item.Equals(itemSlot.item))
                {
                    ItemSlot temp = slots[i];
                    itemSlot.amount -= (ushort)temp.IncreaseAmount(itemSlot.amount);
                    slots[i] = temp;
                }

                // were we able to fit the whole amount already? then stop loop
                if (itemSlot.amount <= 0) isDone = true;
            }

            if (!isDone)
            {
                // add to empty slots (if any)
                for (int i = 0; i < slots.Count; ++i)
                {
                    // empty? then fill slot with as many as possible
                    if (slots[i].amount == 0)
                    {
                        ushort add = (ushort)Mathf.Min(itemSlot.amount, itemSlot.item.maxStack);
                        slots[i] = itemSlot;
                        itemSlot.amount -= add;
                    }

                    // were we able to fit the whole amount already? then stop loop
                    if (itemSlot.amount <= 0) isDone = true;
                }
            }

            // we should have been able to add all of them
            if (!isDone && itemSlot.amount != 0) Debug.LogError("inventory add failed: " + itemSlot.item.name + " " + itemSlot.amount);
        }
    }
}