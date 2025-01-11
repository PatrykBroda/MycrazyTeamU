using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcStorage : NpcOffer
    {
        public override bool HasOffer(Player player) => true;

        public override string GetOfferName() => Localization.Translate("Storage");

        public override void OnSelect(Player player)
        {
            UIMainPanel.singleton.Show();
            UIStorageExtended.singleton.Show(player);
        }
    }
}