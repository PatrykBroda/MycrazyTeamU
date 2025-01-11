using Mirror;

namespace uSurvival
{
    public partial class PlayerEquipment
    {
        [Command]
        public void CmdSwapFromInventoryToEquip(int inventoryIndex, int equipmentIndex)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and in the equipment
            if (health.current > 0 &&
                player.InventorySizeOperationsAllowed(equipmentIndex, inventoryIndex) &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < slots.Count)
            {
                // item slot has to be empty (unequip) or equipable
                ItemSlot slot = inventory.slots[inventoryIndex];
                if (slot.amount > 0 && slot.item.data is UsableItem usableItem && usableItem.CanEquip(this, inventoryIndex, equipmentIndex))
                {
                    ((PlayerInventory)inventory).RpcUsedItem(inventory.slots[inventoryIndex].item);

                    // swap them
                    ItemSlot temp = slots[equipmentIndex];
                    slots[equipmentIndex] = slot;
                    inventory.slots[inventoryIndex] = temp;               

                    //customization addon
                    SwapInventoryEquipForCustomization(inventoryIndex, equipmentIndex);

                    //increase inventory addon (after customization addon)
                    player.inventory.UpdateInventorySize(inventoryIndex, equipmentIndex);
                }
            }
        }

        [Command]
        public void CmdSwapFromEquipToInventory(int inventoryIndex, int equipmentIndex)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and in the equipment
            if (health.current > 0 &&
                player.InventorySizeOperationsAllowed(equipmentIndex, inventoryIndex) &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < slots.Count)
            {
                // item slot has to be empty (unequip) or equipable
                ItemSlot slot = inventory.slots[inventoryIndex];
                if (slot.amount == 0 ||
                slot.item.data is UsableItem usableItem && usableItem.CanEquip(this, inventoryIndex, equipmentIndex))
                {
                    RpcUsedItem(slots[equipmentIndex].item);

                    // swap them
                    ItemSlot temp = slots[equipmentIndex];
                    slots[equipmentIndex] = slot;
                    inventory.slots[inventoryIndex] = temp;

                    //customization addon
                    SwapInventoryEquipForCustomization(inventoryIndex, equipmentIndex);

                    //increase inventory addon (after customization addon)
                    player.inventory.UpdateInventorySize(inventoryIndex, equipmentIndex);
                }
            }
        }

    }
}