using GFFAddons;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace uSurvival
{
    [DisallowMultipleComponent]
    public partial class PlayerAuction : NetworkBehaviour
    {
        public ScriptableAuctionItems itemTypes;

        public enum SearchState { byName, byLeftPanel }
        [HideInInspector] public SearchState searchState;

        [Header("Components")]
        public Player player;

        [Header("Settings")]
        public float refreshTimeout = 5;    // waiting time between attempts to update all items
        public double refreshTimeoutEnd;
        [SerializeField] private uint registrationTax = 0;
        [SerializeField] private float salesTax = 0;
        [HideInInspector] public int numberSalesValue = 0;
        public bool useFavoritesSearch = true;
        public float infoMessageTime = 3;       // the time that will be displayed informational message

        [Header("Settings : Upgrade addon")]
        public bool upgradeAddonUseHoles;
        public bool upgradeAddonPaintRunes = true;
        [SerializeField] private bool _useUpgradeAddon;
        public bool useUpgradeAddon => _useUpgradeAddon;

        [HideInInspector] public int registrationIndex = -1;
        [HideInInspector] public int registrationIndexOld = -1;   // for update InputField Amount if drag new item

        [HideInInspector] public readonly SyncListAuction myItems = new SyncListAuction();
        [HideInInspector] public readonly SyncListAuction allItems = new SyncListAuction();
        [HideInInspector] public readonly SyncList<string> favorites = new SyncList<string>();

        public uint GetRegistrationTax()
        {
            return registrationTax;
        }
        public float GetSalesTax()
        {
            return salesTax;
        }

        private int GetItemPositionInMyList(int id)
        {
            // (avoid FindIndex to minimize allocations)
            for (int i = 0; i < myItems.Count; ++i)
                if (myItems[i].id == id)
                    return i;
            return -1;
        }
        private int GetItemPositionInAllList(int id)
        {
            // (avoid FindIndex to minimize allocations)
            for (int i = 0; i < allItems.Count; ++i)
                if (allItems[i].id == id)
                    return i;
            return -1;
        }

        void OnDragAndDrop_InventorySlot_AuctionSlot(int[] slotIndices)
        {
            if (player.inventory.slots[slotIndices[0]].item.data.auction) registrationIndex = slotIndices[0];
        }
        void OnDragAndDrop_InventoryExtendedSlot_AuctionSlot(int[] slotIndices)
        {
            if (player.inventory.slots[slotIndices[0]].item.data.auction) registrationIndex = slotIndices[0];
        }

        [Command]
        public void CmdLoadMyItemsOnAuction()
        {
            Database.singleton.CharacterLoadMyItemsOnAuction(player);
        }

        [Command]
        public void CmdRegistrationNewItem(int inventoryIndex, ushort amount, uint price)
        {
            if (inventoryIndex <= player.inventory.slots.Count && amount > 0 &&
                amount <= player.inventory.slots[inventoryIndex].amount && price > 0)
            {
                ItemSlot temp = player.inventory.slots[inventoryIndex];

                //decrease item amount in inventory
                temp.amount = (ushort)(player.inventory.slots[inventoryIndex].amount - amount);
                player.inventory.slots[inventoryIndex] = temp;

                //update item in database
                Database.singleton.CharacterSave(player, false);

                //registartion amount
                temp.amount = amount;

                Database.singleton.RegistrationItemOnAuction(name, temp, price);
                Database.singleton.CharacterLoadMyItemsOnAuction(player);
            }
        }

        [Command]
        public void CmdUnregisterItem(int id)
        {
            int positionInList = GetItemPositionInMyList(id);

            if (positionInList != -1 && positionInList <= myItems.Count &&
                player.inventory.CanAdd(player.auction.myItems[positionInList].itemslot.item, player.auction.myItems[positionInList].itemslot.amount))
            {
                if (Database.singleton.UnregisterItemFromAuction(player.auction.myItems[positionInList].id))
                {
                    //add item to inventory
                    player.inventory.Add(myItems[positionInList].itemslot.item, myItems[positionInList].itemslot.amount, false);

                    //remove from auction list
                    myItems.RemoveAt(positionInList);

                    //update item in database
                    Database.singleton.CharacterSave(player, false);
                }
                else Debug.LogWarning("item already sold");

                //update data
                Database.singleton.CharacterLoadMyItemsOnAuction(player);
            }
        }

        [Command]
        public void CmdGetGoldForSoldGoods(int id)
        {
            int positionInList = GetItemPositionInMyList(id);

            //is this item really sold?
            if (positionInList != -1 && positionInList <= myItems.Count && !string.IsNullOrEmpty(player.auction.myItems[positionInList].buyer))
            {
                //update database
                Database.singleton.GetMoneyAuction(myItems[positionInList].id);

                //add gold
                if (player.auction.salesTax > 0)
                {
                    uint tax = (uint)((myItems[positionInList].price / 100) * player.auction.salesTax);
                    player.gold += (myItems[positionInList].price - tax);
                }
                else player.gold += myItems[positionInList].price;

                //remove from auction list
                myItems.RemoveAt(positionInList);

                //update item in database
                Database.singleton.CharacterSave(player, false);
            }
        }

        [Command]
        public void CmdSearchItemsByCategory(string category, string subCategory)
        {
            Debug.Log(player.name + " Try search item on auction by category");
            SearchItemsByCategory(category, subCategory);
        }

        [Server]private void SearchItemsByCategory(string category, string subCategory)
        {
            allItems.Clear();
            foreach (Database.character_auction row in Database.singleton.ItemSearchByCategory(category, subCategory))
            {
                ItemSlot temp = new ItemSlot();
                if (ScriptableItem.dict.TryGetValue(row.item.GetStableHashCode(), out ScriptableItem itemData))
                {
                    temp.item = new Item(itemData);
                    temp.amount = row.amount;

                    // addon system hooks (durability, upgrade)
                    temp.item = UtilsExtended.InvokeManyItem(typeof(PlayerAuction), this, "LoadAuctionFromDatabase_", temp.item, row);
                }

                allItems.Add(new Auction(row.id,
                    row.owner,
                    row.dateRegistration.ToLongDateString(),
                    row.buyer,
                    row.dateBuy.ToLongDateString(),
                    row.getMoney,
                    row.category,
                    row.subcategory,
                    row.price,
                    temp));
            }
        }

        [Command]public void CmdSearchItemsByName(string itemname)
        {
            Debug.Log(player.name + " Try search item on auction by item name");
            if (string.IsNullOrEmpty(itemname))
            SearchItemsByName(itemname);
        }
        private void SearchItemsByName(string itemname)
        {
            allItems.Clear();
            foreach (Database.character_auction row in Database.singleton.ItemSearchByName(itemname))
            {
                ItemSlot temp = new ItemSlot();
                if (ScriptableItem.dict.TryGetValue(row.item.GetStableHashCode(), out ScriptableItem itemData))
                {
                    // addon system hooks (durability, upgrade)
                    temp.item = UtilsExtended.InvokeManyItem(typeof(PlayerAuction), this, "LoadAuctionFromDatabase_", temp.item, row);

                    temp.item = new Item(itemData);
                    temp.amount = row.amount;
                }

                allItems.Add(new Auction(row.id,
                    row.owner,
                    row.dateRegistration.ToLongDateString(),
                    row.buyer,
                    row.dateBuy.ToLongDateString(),
                    row.getMoney,
                    row.category,
                    row.subcategory,
                    row.price,
                    temp));
            }
        }

        [Command]public void CmdBuyItem(int id, SearchState searchMode, string itemname)
        {
            Debug.Log(player.name + " Try buy Item On Auction");

            int positionInList = GetItemPositionInAllList(id);

            //enough space in inventory?
            if (player.inventory.CanAdd(allItems[positionInList].itemslot.item, allItems[positionInList].itemslot.amount))
            {
                //check if this item has been bought yet
                Database.character_auction row = Database.singleton.GetItemInformationOnAuction(allItems[positionInList].id);

                if (row != null)
                {
                    //enough gold to buy ?
                    if (player.gold >= row.price)
                    {
                        //update on the database
                        Database.singleton.BuyItem(player.name, id);

                        player.inventory.Add(allItems[positionInList].itemslot.item, allItems[positionInList].itemslot.amount, false);

                        //update gold
                        player.gold -= row.price;

                        //updating items in the player’s list
                        if (searchMode == SearchState.byName) SearchItemsByName(itemname);
                        else SearchItemsByCategory(allItems[positionInList].itemslot.item.data.GetItemCategory(), allItems[positionInList].itemslot.item.data.GetItemSubCategory());

                        //update data to the seller
                        Player owner = UtilsExtended.FindPlayerByName(row.owner);
                        if (owner != null)
                        {
                            Database.singleton.CharacterLoadMyItemsOnAuction(player);
                        }
                    }
                    else
                    {
                        RpcSendInfoMessageNotEnoughGold();
                    }
                }
                else
                {
                    //by name
                    if (searchMode == SearchState.byName) SearchItemsByName(itemname);
                    else SearchItemsByCategory(allItems[positionInList].itemslot.item.data.GetItemCategory(), allItems[positionInList].itemslot.item.data.GetItemSubCategory());

                    RpcSendInfoMessageAlreadySold();
                }
            }
        }

        //Favorites
        [Command]public void CmdAddToFavorites(string value)
        {
            Debug.Log(player.name + " Try add Item to Auction favorites");
            if (favorites.Contains(value) == false) favorites.Add(value);
        }

        [Command]public void CmdRemoveFromFavorites(string value)
        {
            Debug.Log(player.name + " Try remove Item from Auction favorites");
            favorites.Remove(value);
        }

        //Info Messages
        [ClientRpc]
        void RpcSendInfoMessageNotEnoughGold()
        {
            UIAuctionShowAllItems.singleton.ReceiveMsgNotEnoughGold(player);
        }
        [ClientRpc]
        void RpcSendInfoMessageAlreadySold()
        {
            UIAuctionShowAllItems.singleton.ReceiveMsgAlreadySold(player);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (syncInterval == 0)
            {
                syncInterval = 0.1f;
            }

            player = gameObject.GetComponent<Player>();
            player.auction = this;

            //add events to database
            Database database = FindAnyObjectByType<Database>();
            if (database)
            {
                UnityAction unityAction = new UnityAction(database.Connect_Auction);
                EventsPartial.AddListenerOnceOnConnected(database.onConnected, unityAction, database);

                UnityAction<Player> load = new UnityAction<Player>(database.CharacterLoad_AuctionFavorites);
                EventsPartial.AddListenerOnceCharacterLoad(database.onCharacterLoad, load, database);

                UnityAction<Player> save = new UnityAction<Player>(database.CharacterSave_AuctionFavorites);
                EventsPartial.AddListenerOnceCharacterSave(database.onCharacterSave, save, database);
            }
        }
#endif
    }
}