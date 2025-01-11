using Mirror;
using System;
using UnityEngine;

namespace uSurvival
{
    public partial class Entity
    {
        // all entities should have gold, not just the player
        // useful for monster loot, chests etc.
        // note: int is not enough (can have > 2 mil. easily)
        // note: gold is NOT stored in Inventory.cs because of the single
        //       responsibility principle. also, someone might want multiple
        //       inventories, or inherit from inventory without having gold, etc.
        [Header("Money")] //0 до 4294967295 и занимает 4 байта
        [SyncVar, SerializeField] uint _gold = 0;
        public uint gold { get { return _gold; } set { _gold = Math.Max(value, 0); } }
    }
}


