using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerGuildStorage : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private Player player;
        [SerializeField] private PlayerGuild guild;
        [SerializeField] private PlayerInventory inventory;
        public GuildData data;

        public bool CanBuySlots()
        {
            if (guild.InGuild())
            {
                int requesterIndex = guild.guild.GetMemberIndex(player.name);
                if (requesterIndex != -1)
                {
                    GuildMember requester = guild.guild.members[requesterIndex];
                    return (data.CanBuySlots(requester.rank)) && player.gold >= data.GetStoragePurchaseCost(player.guild.guild) && data.CanBuyMoreSlots(player.guild.guild);
                }
            }

            return false;
        }

        public bool CanPutGoldToStorage()
        {
            if (guild.InGuild())
            {
                int requesterIndex = guild.guild.GetMemberIndex(player.name);
                if (requesterIndex != -1)
                {
                    GuildMember requester = guild.guild.members[requesterIndex];
                    return (data.CanPutGold(requester.rank)) && player.gold > 0;
                }
            }

            return false;
        }

        public bool CanTakeGoldFromStorage()
        {
            if (guild.InGuild())
            {
                int requesterIndex = guild.guild.GetMemberIndex(player.name);
                if (requesterIndex != -1)
                {
                    GuildMember requester = guild.guild.members[requesterIndex];
                    return (data.CanTakeGold(requester.rank)) && player.guild.guild.goldInStorage > 0;
                }
            }

            return false;
        }

        private bool CanTakePutToStorage()
        {
            if (guild.InGuild())
            {
                int requesterIndex = guild.guild.GetMemberIndex(player.name);
                if (requesterIndex != -1)
                {
                    GuildMember requester = guild.guild.members[requesterIndex];
                    return (data.CanPutItems(requester.rank));
                }
            }

            return false;
        }

        private bool CanTakeItemsFromStorage()
        {
            if (guild.InGuild())
            {
                int requesterIndex = guild.guild.GetMemberIndex(player.name);
                if (requesterIndex != -1)
                {
                    GuildMember requester = guild.guild.members[requesterIndex];
                    return (data.CanTakeItems(requester.rank));
                }
            }

            return false;
        }


        void OnDragAndDrop_InventorySlot_StorageGuildSlot(int[] slotIndices)
        {
            if (CanTakePutToStorage())
            {
                CmdSwapInventoryStorageGuild(slotIndices[0], slotIndices[1]);
            }
            else UIGuildStorage.singleton.panelPermissionDenied.SetActive(true);
        }
        void OnDragAndDrop_InventoryExtendedSlot_StorageGuildSlot(int[] slotIndices)
        {
            if (CanTakePutToStorage())
            {
                CmdSwapInventoryStorageGuild(slotIndices[0], slotIndices[1]);
            }
            else UIGuildStorage.singleton.panelPermissionDenied.SetActive(true);
        }
        void OnDragAndDrop_StorageGuildSlot_InventorySlot(int[] slotIndices)
        {
            if (CanTakeItemsFromStorage())
            {
                CmdSwapStorageGuildInventory(slotIndices[0], slotIndices[1]);
            }
            else UIGuildStorage.singleton.panelPermissionDenied.SetActive(true);
        }
        void OnDragAndDrop_StorageGuildSlot_InventoryExtendedSlot(int[] slotIndices)
        {
            if (CanTakeItemsFromStorage())
            {
                CmdSwapStorageGuildInventory(slotIndices[0], slotIndices[1]);
            }
            else UIGuildStorage.singleton.panelPermissionDenied.SetActive(true);
        }
        void OnDragAndDrop_StorageGuildSlot_StorageGuildSlot(int[] slotIndices)
        {
            if (CanTakeItemsFromStorage())
            {
                // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
                if (player.guild.guild.slots[slotIndices[0]].amount > 0 && player.guild.guild.slots[slotIndices[1]].amount > 0 &&
                    player.guild.guild.slots[slotIndices[0]].item.Equals(player.guild.guild.slots[slotIndices[1]].item))
                {
                    CmdStorageGuildMerge(slotIndices[0], slotIndices[1]);
                }
                // split?
                else if (uSurvival.Utils.AnyKeyPressed(player.inventory.splitKeys)) CmdStorageGuildSplit(slotIndices[0], slotIndices[1]);
                // swap?
                else CmdSwapStorageGuildStorageGuild(slotIndices[0], slotIndices[1]);
            }
            else UIGuildStorage.singleton.panelPermissionDenied.SetActive(true);
        }

        [Command]
        public void CmdSwapInventoryStorageGuild(int fromIndex, int toIndex)
        {
            if (player.health.current > 0 &&
                0 <= fromIndex && fromIndex < player.inventory.slots.Count &&
                0 <= toIndex && toIndex < player.guild.guild.slots.Length &&
                player.inventory.slots[fromIndex].item.data.canPutInTheStorageGuild)
            {
                ItemSlot slot = player.inventory.slots[fromIndex];

                ItemSlot[] temp = new ItemSlot[player.guild.guild.storageSize];
                for (int i = 0; i < temp.Length; ++i)
                {
                    temp[i] = player.guild.guild.slots[i];
                }

                // don't allow player to add items which has zero amount or if it's summoned pet item 
                if (slot.amount > 0)
                {
                    // swap them
                    player.inventory.slots[fromIndex] = player.guild.guild.slots[toIndex];
                    temp[toIndex] = slot;

                    GuildSystem.UpdateStorage(player.guild.guild.name, temp);
                }
            }
        }

        [Command]
        public void CmdSwapStorageGuildInventory(int fromIndex, int toIndex)
        {
            if (player.health.current > 0 &&
                0 <= fromIndex && fromIndex < player.guild.guild.slots.Length &&
                0 <= toIndex && toIndex < player.inventory.slots.Count)
            {
                // swap them
                ItemSlot slot = player.guild.guild.slots[fromIndex];

                ItemSlot[] temp = new ItemSlot[player.guild.guild.storageSize];
                for (int i = 0; i < temp.Length; ++i)
                {
                    temp[i] = player.guild.guild.slots[i];
                }

                temp[fromIndex] = player.inventory.slots[toIndex];
                player.inventory.slots[toIndex] = slot;

                GuildSystem.UpdateStorage(player.guild.guild.name, temp);
            }
        }

        [Command]
        public void CmdSwapStorageGuildStorageGuild(int fromIndex, int toIndex)
        {
            // note: should never send a command with complex types!
            // validate: make sure that the slots actually exist in the inventory and that they are not equal
            if (player.health.current > 0 &&
                0 <= fromIndex && fromIndex < player.guild.guild.slots.Length &&
                0 <= toIndex && toIndex < player.guild.guild.slots.Length &&
                fromIndex != toIndex)
            {
                // swap them
                ItemSlot fromIndexslot = player.guild.guild.slots[fromIndex];
                ItemSlot toIndexslot = player.guild.guild.slots[toIndex];

                ItemSlot[] temp = new ItemSlot[player.guild.guild.storageSize];
                for (int i = 0; i < temp.Length; ++i)
                {
                    temp[i] = player.guild.guild.slots[i];
                }

                // swap them
                temp[fromIndex] = toIndexslot;
                temp[toIndex] = fromIndexslot;

                GuildSystem.UpdateStorage(player.guild.guild.name, temp);
            }
        }

        [Command]
        public void CmdStorageGuildSplit(int fromIndex, int toIndex)
        {
            // note: should never send a command with complex types!
            // validate: make sure that the slots actually exist in the inventory
            // and that they are not equal
            if (player.health.current > 0 &&
                0 <= fromIndex && fromIndex < player.guild.guild.slots.Length &&
                0 <= toIndex && toIndex < player.guild.guild.slots.Length &&
                fromIndex != toIndex)
            {
                // slotFrom needs at least two to split, slotTo has to be empty
                ItemSlot slotFrom = player.guild.guild.slots[fromIndex];
                ItemSlot slotTo = player.guild.guild.slots[toIndex];

                if (slotFrom.amount >= 2 && slotTo.amount == 0)
                {
                    ItemSlot[] temp = new ItemSlot[player.guild.guild.storageSize];
                    for (int i = 0; i < temp.Length; ++i)
                        temp[i] = player.guild.guild.slots[i];

                    // split them serversided (has to work for even and odd)
                    slotTo = slotFrom; // copy the value

                    slotTo.amount = (ushort)(slotFrom.amount / 2);
                    slotFrom.amount -= slotTo.amount; // works for odd too

                    // put back into the list
                    temp[fromIndex] = slotFrom;
                    temp[toIndex] = slotTo;

                    GuildSystem.UpdateStorage(player.guild.guild.name, temp);
                }
            }
        }

        [Command]
        public void CmdStorageGuildMerge(int fromIndex, int toIndex)
        {
            if (player.health.current > 0 &&
                0 <= fromIndex && fromIndex < player.guild.guild.slots.Length &&
                0 <= toIndex && toIndex < player.guild.guild.slots.Length &&
                fromIndex != toIndex)
            {
                // both items have to be valid
                ItemSlot slotFrom = player.guild.guild.slots[fromIndex];
                ItemSlot slotTo = player.guild.guild.slots[toIndex];

                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    ItemSlot[] temp = new ItemSlot[player.guild.guild.storageSize];
                    for (int i = 0; i < temp.Length; ++i)
                        temp[i] = player.guild.guild.slots[i];

                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the list
                        temp[fromIndex] = slotFrom;
                        temp[toIndex] = slotTo;

                        GuildSystem.UpdateStorage(player.guild.guild.name, temp);
                    }
                }
            }
        }

        [Command]
        public void CmdGuildStorageBuy()
        {
            Debug.Log("Try buy slots for guild storage");

            //check Permissions
            if (CanBuySlots())
            {
                //is it possible to buy more cells ?
                if (guild.guild.storageSize / data.amountPurchasedSlots < data.storagePurchaseCost.Length)
                {
                    uint price = data.GetPriceForBuingSlots(guild.guild);

                    if (player.gold >= price)
                    {
                        player.gold -= price;
                        GuildSystem.IncreaseStorageSize(guild.guild.name, data.amountPurchasedSlots);
                    }
                }
            }
        }

        [Command]
        public void CmdTransferGoldFromInvToBank(uint gold)
        {
            Debug.Log("Try Transfer Gold To Guild Storage");

            //check Permissions
            if (CanPutGoldToStorage())
            {
                if (gold > 0 && player.gold >= gold)
                {
                    player.gold -= gold;
                    GuildSystem.UpdateGold(guild.guild.name, guild.guild.goldInStorage + gold);
                }
            }
        }

        [Command]
        public void CmdTransferGoldFromBankToInv(uint gold)
        {
            Debug.Log("Try Transfer Gold From Guild Storage");

            //check Permissions
            if (CanTakeGoldFromStorage())
            {
                if (gold > 0 && guild.guild.goldInStorage >= gold)
                {
                    player.gold += gold;
                    GuildSystem.UpdateGold(guild.guild.name, guild.guild.goldInStorage - gold);
                }
            }
        }

        [Command]
        public void CmdTransferItemFromStorage(int fromSlot)
        {
            Debug.Log("Try Get Item from Guild Storage");

            if (CanTakeItemsFromStorage())
            {
                for (int i = 0; i < inventory.slots.Count; i++)
                {
                    if (inventory.slots[i].amount == 0)
                    {
                        ItemSlot temp = inventory.slots[i];
                        inventory.slots[i] = guild.guild.slots[fromSlot];
                        guild.guild.slots[fromSlot] = temp;

                        break;
                    }
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (player == null) player = gameObject.GetComponent<Player>();
            if (guild == null) guild = gameObject.GetComponent<PlayerGuild>();
            player.storageGuild = this;
        }
#endif
    }
}