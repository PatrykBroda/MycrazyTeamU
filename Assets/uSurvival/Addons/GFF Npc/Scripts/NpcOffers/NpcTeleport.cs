using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcTeleport : NpcOffer
    {
        [Header("Teleportation")]
        public Transform destination;

        public override bool HasOffer(Player player) =>
            destination != null;

        public override string GetOfferName() =>
            "Teleport: " + destination.name;

        public override void OnSelect(Player player)
        {
            player.CmdNpcTeleport();
        }
    }
}