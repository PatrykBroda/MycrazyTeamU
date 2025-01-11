using uSurvival;

namespace GFFAddons
{
    public class NpcJoinGroup : NpcOffer
    {
        public Npc owner;

        public override bool HasOffer(Player player) => true;

        public override string GetOfferName() => Localization.Translate("Join");

        public override void OnSelect(Player player)
        {
            player.CmdSetKlan(owner.clan);
        }
    }
}