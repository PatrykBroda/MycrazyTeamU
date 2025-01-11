using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace uSurvival
{
    public class UIConfirmation : MonoBehaviour
    {
        public static UIConfirmation singleton;
        public GameObject panel;
        public TextMeshProUGUI messageText;
        public Button confirmButton;

        public UIConfirmation()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            if (singleton == null) singleton = this;
        }

        public void Show(string message, UnityAction onConfirm)
        {
            messageText.text = message;
            confirmButton.onClick.SetListener(onConfirm);
            panel.SetActive(true);
        }
    }
}