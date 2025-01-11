using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcExchangeCoins : NpcOffer
    {
        public override bool HasOffer(Player player) => true;

        public override string GetOfferName() => Localization.Translate("ExchangeShopCoins");

        public override void OnSelect(Player player)
        {
            UIMainPanel.singleton.Show();
            UIExchangeShopCoins.singleton.Show();
            player.interactionExtended.CmdSetTargetNull();
        }
    }
}