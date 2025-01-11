// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace uSurvival
{
    public partial class UIEquipmentSlot : MonoBehaviour
    {
        public UIEquipmentSlot mainSlot;
        public UIEquipmentSlot secondSlot;

        public Button button;
        public UIShowToolTip tooltip;
        public UIDragAndDropable dragAndDropable;
        public Image image;
        public GameObject categoryOverlay;
        public TextMeshProUGUI categoryText;
        public Image cooldownCircle;
        public GameObject amountOverlay;
        public TextMeshProUGUI amountText;
        public Slider sliderDurability;

        //ammo
        public GameObject ammoIcon;
        public TextMeshProUGUI textAmmoAmount;

        //modules
        public Transform modules;

        //binding
        public GameObject imageBinding;
    }
}