using UnityEngine;
using UnityEngine.UI;

namespace GFFAddons
{
    public class ToolTipItemExtended : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image image;
        public Text textName;
        public GameObject[] panels;
        public Text[] strings;
        public Text[] stringsValue;

        public Text itemEffects;
        public Text itemDescription;

        [Header("GFF Upgrade addon")]
        public GameObject panelUpgrade;
        public Text textUpgradeInfo;
        public Image[] upgradeInd;
    }
}