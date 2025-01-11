using System;
using UnityEngine;
using TMPro;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshPro))]
    public partial class LocalizationTextMesh : MonoBehaviour
    {
        public string key;
        public string specialSymbol;
        private TextMeshPro mainText;

        public void Start()
        {
            mainText = GetComponent<TextMeshPro>();

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

