using System.Text;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "uSurvival Item/Durability Restore Item", order = 999)]
    public class ScriptableDurabilityRestorer : UsableItem
    {
        [SerializeField, Range (0, 1)] private float _durabilityRestorer = 10;
        public float durabilityRestorer => _durabilityRestorer;

        public bool forWeapon;
        public bool forEquipment;

        public override string ToolTip()
        {
            // we use a StringBuilder so that addons can modify tooltips later too
            // ('string' itself can't be passed as a mutable object)
            StringBuilder tip = new StringBuilder(base.ToolTip());
            tip.Replace("{REPAIRKIT}", Localization.Translate("Restore") + " " + (durabilityRestorer*100) + "% " + Localization.Translate("durability"));
            return tip.ToString();
        }

        // usage
        public override Usability CanUseInventory(Player player, int inventoryIndex)
        {
            return Usability.Usable;
        }

        public override void OnUsedInventory(Player player)
        {
            base.OnUsedInventory(player);

            //player.inventory.inventoryIndexDurabilityRestoration = icopy;
            UIItemDurabilityRestoration.singleton.panel.SetActive(true);
        }
    }
}
