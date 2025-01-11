using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class PotionItem
    {
        [Header("GFF Potion Boosts")]
        public byte usageBoost = 0;
    }

    public partial class Player
    {
        public PlayerBoosts boosts;
    }
}



