using System.Collections.Generic;
using TMPro;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class NpcTalk : NpcOffer
    {
        [SerializeField] private ScriptableDialogueData talkData;

        [Header("Text Meshes")]
        public TextMeshPro questOverlay;
        public string textForNewQuest = "!";
        public string textForCompletedQuest = "*";
        public bool useColorsForQuestsTypes = true;

        public override bool HasOffer(Player player) => talkData != null;

        public override string GetOfferName() => Localization.Translate("Talk");

        public override void OnSelect(Player player)
        {
            UINpcTalk.singleton.Show(talkData);
            player.interactionExtended.CmdSetTargetNull();
        }

        private void Update()
        {
            // update overlays in any case, except on server-only mode
            // (also update for character selection previews etc. then)
            if (isServerOnly) return;

            if (questOverlay != null)
            {
                // find local player (null while in character selection)
                if (Player.localPlayer != null)
                {
                    ScriptableQuest completeQuest = CanPlayerCompleteAnyExtendedQuestHere(Player.localPlayer, talkData.dialogues);
                    ScriptableQuest newQuest = CanPlayerAcceptAnyExtendedQuestHere(Player.localPlayer, talkData.dialogues);

                    if (completeQuest != null)
                    {
                        questOverlay.text = textForCompletedQuest;
                        if (useColorsForQuestsTypes) questOverlay.color = completeQuest.color;
                    }
                    else if (newQuest != null)
                    {
                        questOverlay.text = textForNewQuest;
                        if (useColorsForQuestsTypes) questOverlay.color = newQuest.color;
                    }
                    else questOverlay.text = "";
                }
            }
        }

        // overlays ////////////////////////////////////////////////////////////////
        private ScriptableQuest CanPlayerAcceptAnyExtendedQuestHere(Player player, NpcDialogue[] dialogues)
        {
            // check manually. Linq.Any() is HEAVY(!) on GC and performance
            for (int i = 0; i < dialogues.Length; i++)
            {
                if (dialogues[i].quest != null && player.quests.CanAcceptQuestExtended(dialogues[i].quest)) return dialogues[i].quest;
            }
            return null;
        }

        private ScriptableQuest CanPlayerCompleteAnyExtendedQuestHere(Player player, NpcDialogue[] dialogues)
        {
            // check manually. Linq.Any() is HEAVY(!) on GC and performance
            for (int i = 0; i < dialogues.Length; i++)
            {
                if (dialogues[i].quest != null && player.quests.CanCompleteQuest(dialogues[i].quest.name))
                    return dialogues[i].quest;
            }
            return null;
        }
    }
}