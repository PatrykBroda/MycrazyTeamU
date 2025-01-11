// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GFFAddons
{
    public class UIGuildMemberSlot : MonoBehaviour
    {
        public Image onlineStatusImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public Button promoteButton;
        public Button demoteButton;
        public Button kickButton;
        public Button partyButton;
        public TMP_Dropdown dropdown;
    }
}