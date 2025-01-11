using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerPartyOffer : PlayerOffer
    {
        public override bool HasOffer(Player player)
        {
            if (player.party.InParty() == false) return true;
            return false;
        }

        public override string GetOfferName(Player player) => Localization.Translate("Party");

        public override void OnSelect(Player player, Player target)
        {
            if (target != null) player.party.CmdInvite(target.name);
        }
    }
}