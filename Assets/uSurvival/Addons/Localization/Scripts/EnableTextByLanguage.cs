using UnityEngine;
using UnityEngine.UI;

namespace GFFAddons
{
    public class EnableTextByLanguage : MonoBehaviour
    {
        public Text text;
        public SystemLanguage language;

        // Start is called before the first frame update
        void Start()
        {
            Translate();
            Localization.LocalizationChanged += Translate;
        }

        public void OnDestroy()
        {
            Localization.LocalizationChanged -= Translate;
        }

        private void Translate()
        {
            text.enabled = language == Localization.languageCurrent;
        }
    }
}


