using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerFriendsOffer : PlayerOffer
    {
        public override bool HasOffer(Player player) => player.health.current > 0 && NetworkTime.time >= player.nextRiskyActionTime;

        public override string GetOfferName(Player player) => Localization.Translate("AddFriend");

        public override void OnSelect(Player player, Player target)
        {
            if (target != null) player.friends.CmdSendFriendRequest(target.name);
        }
    }
}
