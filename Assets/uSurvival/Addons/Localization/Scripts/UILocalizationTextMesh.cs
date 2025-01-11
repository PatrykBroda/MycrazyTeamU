using System;
using UnityEngine;
using TMPro;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public partial class UILocalizationTextMesh : MonoBehaviour
    {
        public string key;
        public string specialSymbol;
        private TextMeshProUGUI mainText;

        public void Start()
        {
            mainText = GetComponent<TextMeshProUGUI>();

            //if key is null or empty
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)) key = mainText.text;

            Translate();
            Localization.LocalizationChanged += Translate;
        }

        private void Translate()
        {
            string temp = Localization.Translate(key);
            temp = temp.Replace("\\n", Environment.NewLine);
            mainText.text = temp + specialSymbol;
        }

        public void OnDestroy()
        {
            Localization.LocalizationChanged -= Translate;
        }
    }
}


