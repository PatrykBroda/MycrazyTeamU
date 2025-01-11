using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcTrading : NpcOffer
    {
        public ScriptableItem[] saleItems;

        public override bool HasOffer(Player player) => saleItems.Length > 0;

        public override string GetOfferName() => Localization.Translate("Buy/Sell Items");

        public override void OnSelect(Player player)
        {
            UIMainPanel.singleton.Show();
            UINpcTradingExtended.singleton.Show();
            UIInteractionWithEntity.singleton.Hide();   
        }
    }
}