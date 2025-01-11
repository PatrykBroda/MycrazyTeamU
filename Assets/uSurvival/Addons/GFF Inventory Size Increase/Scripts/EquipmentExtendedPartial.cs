using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class EquipmentItem
    {
        [Header("GFF Inventory Size Increase")]
        [SerializeField] private ushort _addSlots;

        public ushort addSlots => _addSlots;
    }

    public partial class Player
    {
        public bool InventorySizeOperationsAllowed(int equipmentIndex, int inventoryIndex)
        {
            //we need to check the number of slots in the inventory - it may decrease
            if (equipment.slots[equipmentIndex].amount > 0 && equipment.slots[equipmentIndex].item.data is EquipmentItem oldbag && oldbag.addSlots > 0)
            {
                //if new item add slots more or equals
                if (inventory.slots[inventoryIndex].amount > 0 && inventory.slots[inventoryIndex].item.data is EquipmentItem newbag && oldbag.addSlots <= newbag.addSlots) return true;

                //check if all backpack slots are empty
                for (int i = 1; i < oldbag.addSlots + 1; i++)
                {
                    if (inventory.slots[inventory.slots.Count - i].amount != 0 || inventory.slots.Count - i == inventoryIndex) return false;
                }
                return true;
            }
            else return true;
        }
    }

    public partial class PlayerInventory
    {
        //public PlayerEquipment equipment;
        [HideInInspector] public ushort sizeDefault = 0;

        private void Awake()
        {
            sizeDefault = size;
        }

        public void LoadInventorySize()
        {
            size += equipment.IncreasingInventoryWithBags();
        }

        [Server]
        public void UpdateInventorySize(int inventoryIndex, int equipmentIndex)
        {
            size = (ushort)(sizeDefault + equipment.IncreasingInventoryWithBags());

            if (size > slots.Count)
            {
                //add slots in list
                for (int i = slots.Count; i < size; ++i)
                    slots.Add(new ItemSlot());
            }
            else if (slots.Count < size)
            {
                if (slots.Count < size)
                {
                    while (slots.Count > size)
                        slots.RemoveAt(slots.Count - 1);
                }
            }
        }
    }

    public partial class PlayerEquipment
    {
        public ushort IncreasingInventoryWithBags()
        {
            ushort amount = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].amount > 0 && slots[i].item.data is EquipmentItem eItem && eItem.addSlots > 0)
                    amount += eItem.addSlots;
            }

            return amount;
        }

        private bool CanDrop(int equipmentIndex)
        {
            //we need to check the number of slots in the inventory - it may decrease
            if (slots[equipmentIndex].item.data is EquipmentItem item && item.addSlots > 0)
            {
                if (inventory.SlotsFree() >= item.addSlots)
                {
                    //check the list of inventory slots from the end
                    //if the slot is busy, we try to move it
                    for (int i = 1; i < item.addSlots + 1; i++)
                    {
                        if (inventory.slots[inventory.slots.Count - i].amount > 0)
                        {
                            int slot = ((PlayerInventory)inventory).FindFreeSlot();
                            inventory.slots[slot] = inventory.slots[inventory.slots.Count - i];
                            inventory.slots[inventory.slots.Count - i] = new ItemSlot();
                        }
                    }

                    return true;
                }
                else return false;
            }
            else return true;
        }

        private void UpdateInventorySizeAfterDeath(int equipmentIndex)
        {
            if (slots[equipmentIndex].amount > 0 && slots[equipmentIndex].item.data is EquipmentItem bagEquip && bagEquip.addSlots > 0)
            {
                ((PlayerInventory)inventory).size -= bagEquip.addSlots;

                if (inventory.slots.Count > ((PlayerInventory)inventory).size)
                {
                    while (inventory.slots.Count > ((PlayerInventory)inventory).size)
                        inventory.slots.RemoveAt(inventory.slots.Count - 1);
                }
            }
        }

        public int AmountStorages()
        {
            int amount = 1;

            for (int i = 0; i < slots.Count; ++i)
            {
                if (slots[i].amount > 0 && slots[i].item.data is EquipmentItem eItem && eItem.addSlots > 0)
                {
                    amount++;
                }
            }

            return amount;
        }
    }

    public partial class Inventory
    {
        public bool CheckBackPackDurability(int index)
        {
            if (this is PlayerInventory inv)
            {
                //in default size ?
                if (index <= inv.sizeDefault) return true;
                else
                {
                    ushort countSlots = inv.sizeDefault;
                    for (short i = 0; i < inv.equipment.slots.Count; ++i)
                    {
                        if (inv.equipment.slots[i].amount > 0 && inv.equipment.slots[i].item.data is EquipmentItem eItem && eItem.addSlots > 0)
                        {
                            countSlots += eItem.addSlots;

                            if (index > countSlots - eItem.addSlots && index <= countSlots)
                            {
                                byte goodSlotsAmount = (byte)(eItem.addSlots / ((float)eItem.maxDurability / inv.equipment.slots[i].item.durability));

                                return index < countSlots - (eItem.addSlots - goodSlotsAmount);
                            }
                        }
                    }
                }

                return false;
            }
            else return true;
        }
    }
}