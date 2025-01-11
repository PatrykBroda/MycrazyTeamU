using TMPro;
using UnityEngine;

namespace GFFAddons
{
    public class UIInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI text;

        //singleton
        public static UIInfoPanel singleton;
        public UIInfoPanel() { singleton = this; }

        public void Show(string message)
        {
            text.text = message;
            panel.SetActive(true);
        }
    }
}