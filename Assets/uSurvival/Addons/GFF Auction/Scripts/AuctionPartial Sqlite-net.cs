using GFFAddons;
using SQLite; // from https://github.com/praeclarum/sqlite-net
using System;
using System.Collections.Generic;

namespace uSurvival
{
    public partial class Database
    {
        public class character_auction
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            public string owner { get; set; }
            public DateTime dateRegistration { get; set; }

            public string buyer { get; set; }
            public DateTime dateBuy { get; set; }
            public bool getMoney { get; set; }

            public string category { get; set; }
            public string subcategory { get; set; }
            public uint price { get; set; }

            public string item { get; set; }
            public ushort amount { get; set; }
            public ushort durability { get; set; }

            //item enchantment addon
            public byte holes { get; set; }
            public string upgradeInd { get; set; }
        }
        public class character_auction_favorites
        {
            public string owner { get; set; }
            public string name { get; set; }
        }

        public void Connect_Auction()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_auction>();
            connection.CreateTable<character_auction_favorites>();
        }

        public void CharacterLoadMyItemsOnAuction(Player player)
        {
            player.auction.myItems.Clear();
            foreach (character_auction row in connection.Query<character_auction>("SELECT * FROM character_auction WHERE owner =? AND getmoney = 0", player.name))
            {
                ItemSlot slot = new ItemSlot();
                if (ScriptableItem.dict.TryGetValue(row.item.GetStableHashCode(), out ScriptableItem itemData))
                {
                    slot.item = new Item(itemData);
                    slot.amount = row.amount;
                    slot.item.durability = row.durability;

                    //item enchantment addon
                    //slot.item.holes = row.holes;
                    //slot.item.upgradeInd = LoadUpgradeInd(row.upgradeInd);
                }

                player.auction.myItems.Add(new Auction(row.id,
                    row.owner,
                    row.dateRegistration.ToLongDateString(),
                    row.buyer,
                    row.dateBuy.ToLongDateString(),
                    row.getMoney,
                    row.category,
                    row.subcategory,
                    row.price,
                    slot));
            }
        }

        public void RegistrationItemOnAuction(string owner, ItemSlot slot, uint price)
        {
            character_auction temp = new character_auction();

            temp.owner = owner;
            temp.dateRegistration = DateTime.Now;
            temp.category = slot.item.data.GetItemCategory();
            temp.subcategory = slot.item.data.GetItemSubCategory();
            temp.price = price;

            temp.item = slot.item.name;
            temp.amount = slot.amount;
            temp.durability = slot.item.durability;

            //item enchantment addon
            //temp.holes = slot.item.holes;
            //temp.upgradeInd = SaveUpgradeInd(slot.item);

            connection.Insert(temp);
        }

        public bool UnregisterItemFromAuction(int id)
        {
            character_auction row = connection.FindWithQuery<character_auction>("SELECT * FROM character_auction WHERE id =? AND buyer is null", id);
            if (row != null)
            {
                connection.Execute("DELETE FROM character_auction WHERE id =?", id);
                return true;
            }
            else return false;
        }

        public void GetMoneyAuction(int id)
        {
            connection.Execute("UPDATE character_auction SET getmoney=1 WHERE id =?", id);
        }

        public List<character_auction> ItemSearchByCategory(string category, string subcategory)
        {
            return connection.Query<character_auction>("SELECT * FROM character_auction WHERE category ='" + category + "' AND subcategory ='" + subcategory + "' AND buyer is null"); ;
        }
        public List<character_auction> ItemSearchByName(string itemName)
        {
            return connection.Query<character_auction>("SELECT * FROM character_auction WHERE buyer is null AND item LIKE '%" + itemName + "%'");
        }

        public void BuyItem(string name, int id)
        {
            connection.Execute("UPDATE character_auction SET buyer =?, datebuy =? WHERE id=?", name, DateTime.Now, id);
        }
        public character_auction GetItemInformationOnAuction(int id)
        {
            return connection.FindWithQuery<character_auction>("SELECT * FROM character_auction WHERE id=? AND buyer is null", id);
        }

        public void CharacterLoad_AuctionFavorites(Player player)
        {
            player.auction.favorites.Clear();
            foreach (character_auction_favorites row in connection.Query<character_auction_favorites>("SELECT * FROM character_auction_favorites WHERE owner =?", player.name))
            {
                player.auction.favorites.Add(row.name);
            }
        }
        public void CharacterSave_AuctionFavorites(Player player)
        {
            connection.Execute("DELETE FROM character_auction_favorites WHERE owner=?", player.name);
            for (int i = 0; i < player.auction.favorites.Count; ++i)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_auction_favorites
                {
                    owner = player.name,
                    name = player.auction.favorites[i]
                });
            }
        }
    }
}


