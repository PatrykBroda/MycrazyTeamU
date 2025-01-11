using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIEquipmentSlotExtended : MonoBehaviour
    {
        public UIShowToolTip tooltip;
        public UIDragAndDropable dragAndDropable;
        public Image image;
        public GameObject categoryOverlay;
        public Text categoryText;
        public Image cooldownCircle;
        public GameObject amountOverlay;
        public Text amountText;
        public Text nameText;
        public Slider sliderDurability;

        //defense
        public GameObject defenseOverlay;
        public TextMeshProUGUI defenseText;

        //ammo
        public TextMeshProUGUI ammoNameText;
        public TextMeshProUGUI ammoCurrentText;
        public TextMeshProUGUI ammoMaxText;
        public GameObject imageAmmo;

        //modules
        public Transform modules;
    }
}


