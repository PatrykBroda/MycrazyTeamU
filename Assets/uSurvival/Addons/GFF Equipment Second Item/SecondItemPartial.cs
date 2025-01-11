using Mirror;

namespace uSurvival
{
    public partial class EquipmentItem
    {
        public string secondCategory;
        public ScriptableItem[] secondItems;

        public bool CanDropToSecondSlot(ScriptableItem data)
        {
            for (int i = 0; i < secondItems.Length; i++)
            {
                if (secondItems[i].Equals(data)) return true;
            }

            return false;
        }
    }

    public partial class PlayerEquipment
    {
        void OnDragAndDrop_InventorySlot_EquipmentSecondSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
            if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].item.data is EquipmentItem eItem &&
                eItem.CanDropToSecondSlot(inventory.slots[slotIndices[0]].item.data))
            {
                CmdSwapInventorySlotEquipmentSecondSlot(slotIndices[0], slotIndices[1]);
            }
        }

        void OnDragAndDrop_EquipmentSecondSlot_InventorySlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
            if (slots[slotIndices[0]].amount > 0 && ScriptableItem.dict.ContainsKey(slots[slotIndices[0]].item.secondItemHash) &&
                (inventory.slots[slotIndices[1]].amount == 0 || ((EquipmentItem)slots[slotIndices[0]].item.data).CanDropToSecondSlot(inventory.slots[slotIndices[1]].item.data)))
            {
                CmdSwapEquipmentSecondSlotInventorySlot(slotIndices[0], slotIndices[1]);
            }
        }

        [Command]
        public void CmdSwapInventorySlotEquipmentSecondSlot(int inventoryIndex, int equipmentIndex)
        {
            if (0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < slots.Count &&
                inventory.slots[inventoryIndex].amount > 0 &&
                slots[equipmentIndex].item.data is EquipmentItem eItem && eItem.CanDropToSecondSlot(inventory.slots[inventoryIndex].item.data))
            {
                // don't allow player to add items which has zero amount
                if (slots[equipmentIndex].amount > 0 && slots[equipmentIndex].item.secondItemHash == 0)
                {
                    ((PlayerInventory)inventory).RpcUsedItem(inventory.slots[inventoryIndex].item);

                    // swap them
                    ItemSlot equipmentSlot = slots[equipmentIndex];
                    equipmentSlot.item.secondItemHash = inventory.slots[inventoryIndex].item.data.name.GetStableHashCode();
                    equipmentSlot.item.secondItemDurability = inventory.slots[inventoryIndex].item.durability;
                    equipmentSlot.item.secondItemBinding = inventory.slots[inventoryIndex].item.binding;
                    slots[equipmentIndex] = equipmentSlot;

                    inventory.slots[inventoryIndex] = new ItemSlot();
                }
            }
        }

        [Command]
        public void CmdSwapEquipmentSecondSlotInventorySlot(int equipmentIndex, int inventoryIndex)
        {
            if (0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < slots.Count &&             
                slots[equipmentIndex].amount > 0)
            {
                if (ScriptableItem.dict.TryGetValue(slots[equipmentIndex].item.secondItemHash, out ScriptableItem secondItemData))
                {
                    ItemSlot equipmentSlot = slots[equipmentIndex];

                    ItemSlot newSlot = new ItemSlot();
                    newSlot.amount = 1;
                    newSlot.item = new Item(secondItemData);
                    newSlot.item.durability = equipmentSlot.item.secondItemDurability;

                    RpcUsedItem(newSlot.item);

                    if (inventory.slots[inventoryIndex].amount < 1)
                    {
                        equipmentSlot.item.secondItemHash = 0;
                        equipmentSlot.item.secondItemDurability = 0;
                        equipmentSlot.item.secondItemBinding = false;
                        slots[equipmentIndex] = equipmentSlot;
                    }
                    else
                    {
                        equipmentSlot.item.secondItemHash = inventory.slots[inventoryIndex].item.data.name.GetStableHashCode();
                        equipmentSlot.item.secondItemDurability = inventory.slots[inventoryIndex].item.durability;
                        equipmentSlot.item.secondItemBinding = inventory.slots[inventoryIndex].item.binding;
                        slots[equipmentIndex] = equipmentSlot;
                    }

                    inventory.slots[inventoryIndex] = newSlot;
                }
            }
        }

        [Server]public void EquipSecondSlotByClick(int inventoryIndex)
        {
            if (0 <= inventoryIndex && inventoryIndex < inventory.slots.Count && inventory.slots[inventoryIndex].amount > 0)
            {
                for (int i = 0; slots.Count > 0; i++)
                {
                    if (slots[i].amount > 0 && slots[i].item.data is EquipmentItem eItem && eItem.CanDropToSecondSlot(inventory.slots[inventoryIndex].item.data))
                    {
                        // swap them
                        ItemSlot newInventorySlot = new ItemSlot();

                        ItemSlot equipmentSlot = slots[i];
                        equipmentSlot.item.secondItemHash = inventory.slots[inventoryIndex].item.data.name.GetStableHashCode();
                        equipmentSlot.item.secondItemDurability = inventory.slots[inventoryIndex].item.durability;
                        equipmentSlot.item.secondItemBinding = inventory.slots[inventoryIndex].item.binding;

                        if (ScriptableItem.dict.TryGetValue(slots[i].item.secondItemHash, out ScriptableItem secondItemData))
                        {
                            newInventorySlot.amount = 1;
                            newInventorySlot.item = new Item(secondItemData);
                            newInventorySlot.item.durability = slots[i].item.secondItemDurability;
                            newInventorySlot.item.binding = slots[i].item.secondItemBinding;
                        }

                        inventory.slots[inventoryIndex] = newInventorySlot;
                        slots[i] = equipmentSlot;

                        break;
                    }
                }
            }
        }
    }
}