using GFFAddons;
using SQLite;
using System.Collections.Generic;
using UnityEngine;

namespace uSurvival
{
    //public partial class Entity
    //{
    //    // all entities should have gold, not just the player
    //    // useful for monster loot, chests etc.
    //    // note: int is not enough (can have > 2 mil. easily)
    //    // note: gold is NOT stored in Inventory.cs because of the single
    //    //       responsibility principle. also, someone might want multiple
    //    //       inventories, or inherit from inventory without having gold, etc.
    //    [Header("Shop Coins")]
    //    [SyncVar, SerializeField] int _coins = 0;
    //    public int coins { get { return _coins; } set { _coins = Math.Max(value, 0); } }
    //}

    public partial class ScriptableItem
    {
        [Header("Shop Addon")]
        public uint itemMallPrice;
    }

    public partial struct Item
    {
        public uint itemMallPrice => data.itemMallPrice;
    }

    public partial class Database
    {
        public uint LoadCoinsForAccount(string account)
        {
            return connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=?", account).coins;
        }

        class character_orders
        {
            // INTEGER PRIMARY KEY is auto incremented by sqlite if the insert call
            // passes NULL for it.
            [PrimaryKey] // important for performance: O(log n) instead of O(n)
            public int orderid { get; set; }
            public string character { get; set; }
            public uint coins { get; set; }
            public string tid { get; set; }
            public bool processed { get; set; }
        }

        public void Connect_ItemMall()
        {
            connection.CreateTable<character_orders>();
        }

        // item mall ///////////////////////////////////////////////////////////////
        public List<uint> GrabCharacterOrders(string characterName)
        {
            // grab new orders from the database and delete them immediately
            //
            // note: this requires an orderid if we want someone else to write to
            // the database too. otherwise deleting would delete all the new ones or
            // updating would update all the new ones. especially in sqlite.
            //
            // note: we could just delete processed orders, but keeping them in the
            // database is easier for debugging / support.
            List<uint> result = new List<uint>();
            List<character_orders> rows = connection.Query<character_orders>("SELECT * FROM character_orders WHERE character=? AND processed=0", characterName);
            foreach (character_orders row in rows)
            {
                result.Add(row.coins);
                connection.Execute("UPDATE character_orders SET processed=1 WHERE orderid=?", row.orderid);
            }
            return result;
        }
    }

    public partial class Player
    {
        [Header("GFF Shop Addon")]
        public PlayerItemMall itemMall;
    }
}

namespace GFFAddons
{
    public partial class Npc
    {
        [Header("Shop Coins")]
        public NpcExchangeCoins exchangeShopCoins;
    }
}
