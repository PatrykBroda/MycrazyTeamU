using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerStorage : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        [Header("Settings")]
        [SerializeField] private uint[] storagePurchaseCost = new uint[5] { 10, 20, 30, 40, 50 };
        [SerializeField] private byte amountPurchasedSlots = 5;
        [SerializeField, Tooltip("if equal to zero, then it will be impossible to store gold")] private long maxGoldInStorage = 2000000000;

        [SyncVar, HideInInspector] public uint goldInStorage = 0;
        [SyncVar, HideInInspector] public byte storageSize = 0;

        public SyncListItemSlot slots = new SyncListItemSlot();

        public uint GetStoragePurchaseCost()
        {
            if (storageSize < storagePurchaseCost.Length * amountPurchasedSlots)
                return storagePurchaseCost[storageSize / amountPurchasedSlots];
            else return 0;
        }
        public int GetMaxSlotsAmountForStorage()
        {
            return storagePurchaseCost.Length * amountPurchasedSlots;
        }
        public bool CanBuySlots()
        {
            return player.gold >= GetStoragePurchaseCost() && CanBuyMoreSlots();
        }
        public bool CanBuyMoreSlots()
        {
            return storageSize < storagePurchaseCost.Length * amountPurchasedSlots;
        }
        public bool CanPutGoldToStorage()
        {
            return storageSize > 0 && goldInStorage < maxGoldInStorage;
        }
        public long GetMaxCapacityForGold()
        {
            return maxGoldInStorage;
        }

        void OnDragAndDrop_StorageSlot_StorageSlot(int[] slotIndices)
        {
            // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
            if (slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
                slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
            {
                CmdStorageMerge(slotIndices[0], slotIndices[1]);
            }
            // split?
            else if (uSurvival.Utils.AnyKeyPressed(player.inventory.splitKeys))
            {
                CmdStorageSplit(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                CmdSwapStorageStorage(slotIndices[0], slotIndices[1]);
            }
        }
        void OnDragAndDrop_InventorySlot_StorageSlot(int[] slotIndices)
        {
            CmdSwapInventoryStorage(slotIndices[0], slotIndices[1]);
        }
        void OnDragAndDrop_InventoryExtendedSlot_StorageSlot(int[] slotIndices)
        {
            CmdSwapInventoryStorage(slotIndices[0], slotIndices[1]);
        }
        void OnDragAndDrop_StorageSlot_InventorySlot(int[] slotIndices)
        {
            CmdSwapStorageInventory(slotIndices[0], slotIndices[1]);
        }
        void OnDragAndDrop_StorageSlot_InventoryExtendedSlot(int[] slotIndices)
        {
            CmdSwapStorageInventory(slotIndices[0], slotIndices[1]);
        }

        [Command]
        public void CmdSwapInventoryStorage(int fromIndex, int toIndex)
        {
            if (player.movement.state == MoveState.IDLE &&
                0 <= fromIndex && fromIndex < player.inventory.slots.Count &&
                0 <= toIndex && toIndex < slots.Count
                && player.inventory.slots[fromIndex].item.data.canPutInTheStorage)
            {
                ItemSlot slot = player.inventory.slots[fromIndex];

                // don't allow player to add items which has zero amount or if it's summoned pet item 
                if (slot.amount > 0)
                {
                    // swap them
                    player.inventory.slots[fromIndex] = slots[toIndex];
                    slots[toIndex] = slot;
                }
            }
        }
        [Command]
        public void CmdSwapStorageInventory(int fromIndex, int toIndex)
        {
            if (player.movement.state == MoveState.IDLE &&
                0 <= fromIndex && fromIndex < slots.Count &&
                0 <= toIndex && toIndex < player.inventory.slots.Count)
            {
                // swap them
                ItemSlot temp = slots[fromIndex];
                slots[fromIndex] = player.inventory.slots[toIndex];
                player.inventory.slots[toIndex] = temp;
            }
        }
        [Command]
        public void CmdSwapStorageStorage(int fromIndex, int toIndex)
        {
            // note: should never send a command with complex types!
            // validate: make sure that the slots actually exist in the inventory
            // and that they are not equal
            if (player.movement.state == MoveState.IDLE &&
                0 <= fromIndex && fromIndex < slots.Count &&
                0 <= toIndex && toIndex < slots.Count &&
                fromIndex != toIndex)
            {
                // swap them
                ItemSlot temp = slots[fromIndex];
                slots[fromIndex] = slots[toIndex];
                slots[toIndex] = temp;
            }
        }
        [Command]
        public void CmdStorageSplit(int fromIndex, int toIndex)
        {
            // note: should never send a command with complex types!
            // validate: make sure that the slots actually exist in the inventory
            // and that they are not equal
            if (player.movement.state == MoveState.IDLE &&
                0 <= fromIndex && fromIndex < slots.Count &&
                0 <= toIndex && toIndex < slots.Count &&
                fromIndex != toIndex)
            {
                // slotFrom needs at least two to split, slotTo has to be empty
                ItemSlot slotFrom = slots[fromIndex];
                ItemSlot slotTo = slots[toIndex];
                if (slotFrom.amount >= 2 && slotTo.amount == 0)
                {
                    // split them serversided (has to work for even and odd)
                    slotTo = slotFrom; // copy the value

                    slotTo.amount = (ushort)(slotFrom.amount / 2);
                    slotFrom.amount -= slotTo.amount; // works for odd too

                    // put back into the list
                    slots[fromIndex] = slotFrom;
                    slots[toIndex] = slotTo;
                }
            }
        }
        [Command]
        public void CmdStorageMerge(int fromIndex, int toIndex)
        {
            if (player.movement.state == MoveState.IDLE &&
                0 <= fromIndex && fromIndex < slots.Count &&
                0 <= toIndex && toIndex < slots.Count &&
                fromIndex != toIndex)
            {
                // both items have to be valid
                ItemSlot slotFrom = slots[fromIndex];
                ItemSlot slotTo = slots[toIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        ushort put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the list
                        slots[fromIndex] = slotFrom;
                        slots[toIndex] = slotTo;
                    }
                }
            }
        }


        [Command]
        public void CmdStorageBuy()
        {
            uint needGold = storagePurchaseCost[storageSize / amountPurchasedSlots];

            if (player.gold >= needGold)
            {
                player.gold -= needGold;
                storageSize += amountPurchasedSlots;

                for (int i = storageSize - amountPurchasedSlots; i < storageSize; ++i)
                    slots.Add(new ItemSlot());
            }
        }
        [Command]
        public void CmdTransferGoldFromStorage(uint value)
        {
            if (value > 0 && goldInStorage >= value)
            {
                player.gold += value;
                goldInStorage -= value;
            }
        }
        [Command]
        public void CmdTransferGoldToStorage(uint value)
        {
            if (value > 0 && player.gold >= value && (goldInStorage + value <= maxGoldInStorage))
            {
                player.gold -= value;
                goldInStorage += value;
            }
        }

        [Command]
        public void CmdTransferItemFromStorage(int fromSlot)
        {
            for (int i = 0; i < player.inventory.slots.Count; i++)
            {
                if (player.inventory.slots[i].amount == 0)
                {
                    ItemSlot temp = player.inventory.slots[i];
                    player.inventory.slots[i] = slots[fromSlot];
                    slots[fromSlot] = temp;

                    break;
                }
            }
        }

        [Command]
        public void CmdStorageSort()
        {
            SyncListItemSlot temp = new SyncListItemSlot();

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].amount > 0) temp.Add(slots[i]);
            }

            while (temp.Count < slots.Count)
            {
                temp.Add(new ItemSlot());
            }

            slots = temp;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            player = gameObject.GetComponent<Player>();
            player.storage = this;
        }
#endif
    }
}