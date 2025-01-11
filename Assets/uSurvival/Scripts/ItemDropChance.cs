// Defines the drop chance of an item for monster loot generation.
using System;
using UnityEngine;

namespace uSurvival
{
    [Serializable]
    public class ItemDropChance
    {
        [HideInInspector] public string name;
        public ItemDrop drop;
        [Range(0,1)] public float probability;

        //gff
        public int minAmount = 1;
        public int maxAmount = 1;
    }
}