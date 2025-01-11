using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcGuildStorage : NpcOffer
    {
        public override bool HasOffer(Player player) => player.guild.InGuild();

        public override string GetOfferName() => Localization.Translate("GuildStorage");

        public override void OnSelect(Player player)
        {
            UIMainPanel.singleton.CloseAllPanels();
            UIMainPanel.singleton.Show();
            UIGuildStorage.singleton.Show(player);
        }
    }
}