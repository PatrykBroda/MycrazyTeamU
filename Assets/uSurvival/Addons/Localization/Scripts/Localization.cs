using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[Serializable]public class LocalizeText
{
    [HideInInspector]public string text;
    public SystemLanguage language;
    [TextArea] public string description;
}

[Serializable]public class Lang
{
    public SystemLanguage language;
    public Font font;
    public TMP_FontAsset fontTMP;
}

public class Localization : MonoBehaviour
{
    public static SystemLanguage languageCurrent = SystemLanguage.English;
    public static event Action LocalizationChanged = () => { };

    public static List<string> languagesFromCsv;
    private static string path = "Localization";
    public static Dictionary<string, Dictionary<string, string>> Dictionary = new Dictionary<string, Dictionary<string, string>>();

    private void OnEnable()
    {
        ReadCSV();
        LanguageDetection();
        Language = languageCurrent;
    }

    private void LanguageDetection()
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            languageCurrent = (SystemLanguage)PlayerPrefs.GetInt("Language");
        }
        else
        {
            if (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Belarusian || Application.systemLanguage == SystemLanguage.Ukrainian)
            {
                languageCurrent = SystemLanguage.Russian;
            }
            else
            {
                if (IsAvailableLanguage(Application.systemLanguage)) languageCurrent = Application.systemLanguage;
                else languageCurrent = SystemLanguage.English;
            }
        }
    }

    private static bool IsAvailableLanguage(SystemLanguage language)
    {
        if (languagesFromCsv.Count == 0) return false;

        return languagesFromCsv.Contains(language.ToString());
    }

    // Read localization spreadsheets.
    private static void ReadCSV()
    {
        if (Dictionary.Count > 0) return;

        // Loading the dataset from Unity's Resources folder
        var csvFiles = Resources.LoadAll<TextAsset>(path);

        foreach (var csvFile in csvFiles)
        {
            string text = csvFile.text;

            string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            languagesFromCsv = lines[0].Split(';').Select(i => i.Trim()).ToList();

            for (int i = 0; i < languagesFromCsv.Count; i++)
            {
                if (!Dictionary.ContainsKey(languagesFromCsv[i]))
                {
                    Dictionary.Add(languagesFromCsv[i], new Dictionary<string, string>());
                }
                else
                {
                    //Debug.LogError("Localization key already added " + availableLanguages[i]);
                }
            }

            for (int i = 1; i < lines.Length; i++)
            {
                List<string> columns = lines[i].Split(';').Select(j => j.Trim()).ToList();
                string key = columns[0];

                if (key == "") continue;

                for (int x = 0; x < languagesFromCsv.Count; x++)
                {
                    if (!Dictionary[languagesFromCsv[x]].ContainsKey(key)) Dictionary[languagesFromCsv[x]].Add(key, columns[x+1]);
                    else
                    {
                        Debug.LogError("Localization key already added " + key);
                    }
                }
            }
        }
    }

    public static string Translate(string localizationKey)
    {
        if (Dictionary.Count == 0) ReadCSV();

        if (!Dictionary.ContainsKey(languageCurrent.ToString()))
        {
            Language = SystemLanguage.English;
            //Debug.LogError("The dictionary does not contain the language you want to translate");
        }

        if (!Dictionary[languageCurrent.ToString()].ContainsKey(localizationKey) || string.IsNullOrEmpty(Dictionary[languageCurrent.ToString()][localizationKey]))
        {         
            return Dictionary["English"].ContainsKey(localizationKey) ? Dictionary["English"][localizationKey] : localizationKey;
        }
        else
        {
            return Dictionary[languageCurrent.ToString()][localizationKey];
        }
    }

    public static SystemLanguage Language
    {
        get
        {
            return languageCurrent;
        }
        set
        {
            languageCurrent = value;
            LocalizationChanged();
            PlayerPrefs.SetInt("Language", (int)Language);
        }
    }
}