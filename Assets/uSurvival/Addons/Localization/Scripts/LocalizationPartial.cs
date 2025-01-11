using UnityEngine;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        public LocalizeText[] localization;

        public string GetDescriptionByLanguage(SystemLanguage lang)
        {
            for (int i = 0; i < localization.Length; i++)
            {
                if (localization[i].language == lang) return localization[i].description;
            }
            return null;
        }
    }
}