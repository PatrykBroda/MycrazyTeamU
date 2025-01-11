using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public partial class UniversalSlot : MonoBehaviour
    {
        public UIShowToolTip tooltip;
        public Button button;
        public UIDragAndDropable dragAndDropable;
        public Image image;
        public Image cooldownCircle;
        public GameObject amountOverlay;
        public TextMeshProUGUI amountText;

        [Header("Rarity Addon")]
        public Image rarityImage;

        [Header("Item Enchantment Addon")]
        public Text upgradeText;

        [Header("only for Equipment")]
        public GameObject categoryOverlay;
        public Text categoryText;

        [Header("only for Skillbar")]
        public Text hotkeyText;

        [Header("only for skills")]
        public GameObject cooldownOverlay;
        public Text cooldownText;

        [Header("only for loot")]
        public Text nameText;

        [Header("only for macro")]
        public float CooldownTime;
        public float CooldownRemainingTime;

        //[Header("only for quest")]
        //public Image border;

        public Slider sliderDurability;
        public TextMeshProUGUI textDurability;
        public GameObject imageBinding;
        public TextMeshProUGUI textAmmoType;
    }
}