// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GFFAddons
{
    public class UIPartyExtendedMemberSlot : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI masterIndicatorText;

        public Slider healthSlider;
        public Image healthDistanceImage;

        public Button buttonChangeLeader;
        public Button buttonKickFromParty;
    }
}