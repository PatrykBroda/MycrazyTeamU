using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcGuildManagement : NpcOffer
    {
        public override bool HasOffer(Player player) => true;

        public override string GetOfferName() => Localization.Translate("GuildManagement");

        public override void OnSelect(Player player)
        {
            UIMainPanel.singleton.CloseAllPanels();
            UINpcGuildManagement.singleton.Show();
            UIMainPanel.singleton.Show();
        }
    }
}