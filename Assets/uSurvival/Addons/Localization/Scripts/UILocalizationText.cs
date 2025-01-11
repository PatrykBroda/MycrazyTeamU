using System;
using UnityEngine;
using UnityEngine.UI;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public partial class UILocalizationText : MonoBehaviour
    {
        [SerializeField] private string key;
        [SerializeField] private string specialSymbol;
        [SerializeField] private string specialSymbolEnd;
        private Text mainText;

        public void Start()
        {
            mainText = GetComponent<Text>();

            //if key is null or empty
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)) key = mainText.text;

            Translate();
            Localization.LocalizationChanged += Translate;
        }

        private void Translate()
        {
            string temp = Localization.Translate(key);
            temp = temp.Replace("\\n", Environment.NewLine);
            mainText.text = specialSymbol + temp + specialSymbolEnd;
        }

        public void OnDestroy()
        {
            Localization.LocalizationChanged -= Translate;
        }
    }
}


