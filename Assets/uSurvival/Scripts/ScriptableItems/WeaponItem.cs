// common type for all kinds of weapons. we need a common type to check what's
// allowed on the hotbar, etc.
using System.Text;
using UnityEngine;

namespace uSurvival
{
    public abstract partial class WeaponItem : EquipmentItem
    {
        [Header("Weapon")]
        public float attackRange = 20; // attack range
        public short damage = 10;
        public string upperBodyAnimationParameter;
        public Sprite imageHorizontal;

        // usage: disable inventory usage for weapons. only from hotbar.
        // (right clicking a rifle in the inventory to shoot it would be odd)
        //public override Usability CanUseInventory(Player player, int inventoryIndex) { return Usability.Never; }
        //public override void UseInventory(Player player, int inventoryIndex) {}

        // tooltip
        public override string ToolTip()
        {
            StringBuilder tip = new StringBuilder(base.ToolTip());
            tip.Replace("{ATTACKRANGE}", attackRange.ToString());
            tip.Replace("{DAMAGE}", Localization.Translate("Damage") + ": " + damage.ToString());
            return tip.ToString();
        }
    }
}