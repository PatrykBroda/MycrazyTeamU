using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GFFAddons;
using Mirror;

namespace uSurvival
{
    public class UIPopup : MonoBehaviour
    {
        public static UIPopup singleton;
        public GameObject panel;
        public TextMeshProUGUI messageText;
        public Button buttonClose;

        public UIPopup()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            if (singleton == null) singleton = this;
        }

        public void Show(string message)
        {
            // append error if visible, set otherwise. then show it.
            if (panel.activeSelf) messageText.text += $";\n{message}";
            else messageText.text = message;
            panel.SetActive(true);
        }

        private void Start()
        {
            buttonClose.onClick.SetListener(() =>
            {
                NetworkManagerSurvival.Quit();
            });
        }
    }
}