using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcAuction : NpcOffer
    {
        public override bool HasOffer(Player player) => true;

        public override string GetOfferName() => Localization.Translate("Auction");

        public override void OnSelect(Player player)
        {
            player.auction.CmdLoadMyItemsOnAuction();
            UIAuctionShowAllItems.singleton.Show(player);
            UIInteractionWithEntity.singleton.Hide();
        }
    }
}