using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerGuildOffer : PlayerOffer
    {
        public override bool HasOffer(Player player)
        {
            if (player.interactionExtended.target != null && player.interactionExtended.target is Player otherPlayer)
            {
                if (player.guild.InGuild() && otherPlayer.guild.InGuild() == false && player.guild.CanAcceptToGuild()) return true;
                if (player.guild.InGuild() == false && otherPlayer.guild.InGuild()) return true;
            }

            return false;
        }

        public override string GetOfferName(Player player)
        {
            if (player.guild.InGuild()) return Localization.Translate("GuildInvite");
            else return Localization.Translate("GuildJoin");
        }

        public override void OnSelect(Player player, Player target)
        {
            if (target != null)
            {
                if (player.guild.InGuild() && target.guild.InGuild() == false && player.guild.CanAcceptToGuild()) player.guild.CmdInviteTarget();
                if (player.guild.InGuild() == false && target.guild.InGuild()) player.guild.CmdRequestToJoinTheGuild(target.guild.guild.name);
            }
        }
    }
}
