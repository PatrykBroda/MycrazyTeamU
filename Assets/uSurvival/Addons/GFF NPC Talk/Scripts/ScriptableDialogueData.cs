using System;
using System.Collections.Generic;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [Serializable]
    public class NpcDialogue
    {
        public string name;
        public LocalizeText[] descritions;
        public ScriptableDialogueData continuedDialogue;

        public ScriptableQuest quest;
        public bool acceptHere = true;
        public bool completeHere = true;

        public string GetDialogueLocalization(SystemLanguage lang)
        {
            for (int i = 0; i < descritions.Length; i++)
            {
                if (descritions[i].language == lang) return descritions[i].description;
            }
            return descritions[0].description;
        }
    }

    [CreateAssetMenu(menuName = "GFF Addons/Npc Dialogue Data", order = 999)]
    public class ScriptableDialogueData : ScriptableObject
    {
        public ScriptableDialogueData predecessor;
        public LocalizeText[] welcomeText;
        public NpcDialogue[] dialogues;

        public string GetWelcomeTextByLanguage(SystemLanguage lang)
        {
            for (int i = 0; i < welcomeText.Length; i++)
            {
                if (welcomeText[i].language == lang) return welcomeText[i].description;
            }
            return "";
        }

        public List<NpcDialogue> GetInteractions(Player player, SystemLanguage lang)
        {
            List<NpcDialogue> interactionsSorted = new List<NpcDialogue>();

            for (int i = 0; i < dialogues.Length; i++)
            {
                if (dialogues[i].quest == null) interactionsSorted.Add(dialogues[i]);
                else
                {
                    if (dialogues[i].acceptHere && player.quests.CanAcceptQuestExtended(dialogues[i].quest)) interactionsSorted.Add(dialogues[i]);
                    else
                    {
                        int questIndex = player.quests.GetIndexByName(dialogues[i].quest.name);
                        if (questIndex != -1 && !player.quests.quests[questIndex].completed) interactionsSorted.Add(dialogues[i]);
                    }
                }
            }

            return interactionsSorted;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (welcomeText != null && welcomeText.Length > 0)
            {
                for (int i = 0; i < welcomeText.Length; i++)
                {
                    welcomeText[i].text = welcomeText[i].language.ToString();
                }
            }

            if (dialogues != null && dialogues.Length > 0)
            {
                for (int i = 0; i < dialogues.Length; i++)
                {
                    if (dialogues[i].descritions != null && dialogues[i].descritions.Length > 0)
                    {
                        for (int x = 0; x < dialogues[i].descritions.Length; x++)
                        {
                            dialogues[i].descritions[x].text = dialogues[i].descritions[x].language.ToString();
                        }
                    }
                }
            }
        }
#endif
    }
}