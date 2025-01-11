using UnityEngine;
using uSurvival;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        [Header("GFF Opportunity sell to Auction")]
        [SerializeField] private bool _auction = true;

        public bool auction => _auction;
    }

    public partial class Player
    {
        [Header("GFF Auction Addon")]
        public PlayerAuction auction;
    }
}


namespace GFFAddons
{
    public partial class UIInventoryExtended
    {
        void IsUsedSlot_Auction(Player player, int inventoryIndex)
        {
            if (isUsedSlot == false)
            {
                if (player.auction.registrationIndex == inventoryIndex) isUsedSlot = true;
            }
        }
    }
}