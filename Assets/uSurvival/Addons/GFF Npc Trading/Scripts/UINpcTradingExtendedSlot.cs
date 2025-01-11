using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UINpcTradingExtendedSlot : MonoBehaviour
    {
        public UIShowToolTip tooltip;
        public Button button;
        public UIDragAndDropable dragAndDropable;
        public Image image;
        public GameObject amountOverlay;
        public Text amountText;

        [Header("Rarity Addon")]
        public Image rarityImage;

        [Header("Item Enchantment Addon")]
        public Text upgradeText;

        [Header("GFF ToolTip")]
        public UIShowToolTipExtended tooltipExtended;
    }
}


