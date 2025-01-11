using UnityEngine;
using Mirror;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        [SerializeField] private bool _autoBind;
        public bool autoBind => _autoBind;
    }

    public partial class Inventory
    {
        public bool Add(Item item, ushort amount, bool binding)
        {
            // we only want to add them if there is enough space for all of them, so
            // let's double check
            if (CanAdd(item, amount))
            {
                // add to same item stacks first (if any)
                // (otherwise we add to first empty even if there is an existing
                //  stack afterwards)
                for (int i = 0; i < slots.Count; ++i)
                {
                    // not empty and same type? then add free amount (max-amount)
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slots[i].amount > 0 && slots[i].item.data.Equals(item.data))
                    {
                        ItemSlot temp = slots[i];
                        amount -= temp.IncreaseAmount(amount);
                        slots[i] = temp;
                    }

                    // were we able to fit the whole amount already? then stop loop
                    if (amount <= 0) return true;
                }

                // add to empty slots (if any)
                for (int i = 0; i < slots.Count; ++i)
                {
                    // empty? then fill slot with as many as possible
                    if (slots[i].amount == 0)
                    {
                        ushort add = (ushort)Mathf.Min(amount, item.maxStack);

                        ItemSlot slot = new ItemSlot();
                        slot.item = item;
                        slot.amount = add;
                        slot.item.binding = binding;

                        slots[i] = slot;
                        amount -= add;
                    }

                    // were we able to fit the whole amount already? then stop loop
                    if (amount <= 0) return true;
                }
                // we should have been able to add all of them
                if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
            }
            return false;
        }
    }

    public partial class PlayerInventory
    {
        [Header("Item Bind")]
        public ScriptableItemAndAmount requeredItemForbind;
        public int itemIndexForBind = -1;

        void OnDragAndDrop_InventorySlot_BindSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            if (slots[slotIndices[0]].amount > 0 && (slots[slotIndices[0]].item.data is EquipmentItem || slots[slotIndices[0]].item.data is WeaponItem))
            {
                itemIndexForBind = slotIndices[0];
            }
        }

        [Command]
        public void CmdBindItem(int index)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and that they are not equal
            if (player.health.current > 0 && 0 <= index && index < slots.Count &&
                slots[index].amount > 0 && (slots[index].item.data is EquipmentItem || slots[index].item.data is WeaponItem) &&
                Count(requeredItemForbind.item) >= requeredItemForbind.amount)
            {
                ItemSlot temp = slots[index];
                temp.item.binding = !slots[index].item.binding;
                slots[index] = temp;

                Remove(requeredItemForbind.item, requeredItemForbind.amount);
            }
        }
    }
}