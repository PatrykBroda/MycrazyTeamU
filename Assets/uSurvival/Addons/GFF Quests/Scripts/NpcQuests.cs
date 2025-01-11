using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using uSurvival;

namespace GFFAddons
{
    // talk-to-npc quests work by adding the same quest to two npcs, one with
    // accept=true and complete=false, the other with accept=false and complete=true
    [Serializable]
    public class ScriptableQuestOffer
    {
        [HideInInspector] public string name;
        public ScriptableQuest quest;
        public bool acceptHere = true;
        public bool completeHere = true;
    }

    [DisallowMultipleComponent]
    public partial class NpcQuests : NpcOffer
    {
        [Header("Text Meshes")]
        public TextMeshPro questOverlay;
        public string textForNewQuest = "!";
        public string textForCompletedQuest = "*";
        public bool useColorsForQuestsTypes = true;

        [Header("Quests")]
        public ScriptableQuestOffer[] quests;

        private List<ScriptableQuest> visibleQuests = new List<ScriptableQuest>();

        public override bool HasOffer(Player player) =>
            QuestsVisibleFor(player).Count > 0;

        public override string GetOfferName() => Localization.Translate("Quests");

        public override void OnSelect(Player player)
        {
            UIQuestsByNpc.singleton.Show();
            //player.interactionExtended.CmdSetTargetNull();
        }

        // helper function to find a quest index by name
        public int GetIndexByName(string questName)
        {
            // (avoid Linq because it is HEAVY(!) on GC and performance)
            for (int i = 0; i < quests.Length; ++i)
                if (quests[i].quest.name == questName)
                    return i;
            return -1;
        }

        // helper function to filter the quests that are shown for a player
        // -> all quests that:
        //    - can be started by the player
        //    - or were already started but aren't completed yet
        public List<ScriptableQuest> QuestsVisibleFor(Player player)
        {
            // search manually. Linq is HEAVY(!) on GC and performance
            visibleQuests.Clear();
            foreach (ScriptableQuestOffer entry in quests)
            {
                if (entry.acceptHere && player.quests.CanAcceptQuestExtended(entry.quest) ||
                    entry.completeHere && player.quests.HasActive(entry.quest.name))
                    visibleQuests.Add(entry.quest);
            }

            return visibleQuests;
        }

        // overlays ////////////////////////////////////////////////////////////////
        private ScriptableQuest CanPlayerAcceptAnyExtendedQuestHere(Player player)
        {
            // check manually. Linq.Any() is HEAVY(!) on GC and performance
            for (int i = 0; i < quests.Length; i++)
            {
                ScriptableQuestOffer entry = quests[i];
                if (entry.acceptHere && player.quests.CanAcceptQuestExtended(entry.quest)) return entry.quest;
            }
            return null;
        }

        private ScriptableQuest CanPlayerCompleteAnyExtendedQuestHere(Player player)
        {
            // check manually. Linq.Any() is HEAVY(!) on GC and performance
            for (int i = 0; i < quests.Length; i++)
            {
                ScriptableQuestOffer entry = quests[i];
                if (entry.completeHere && player.quests.CanCompleteQuest(entry.quest.name))
                    return entry.quest;
            }
            return null;
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
                    ScriptableQuest completeQuest = CanPlayerCompleteAnyExtendedQuestHere(Player.localPlayer);
                    ScriptableQuest newQuest = CanPlayerAcceptAnyExtendedQuestHere(Player.localPlayer);

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

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            gameObject.GetComponent<Npc>().quests = this;

            for (int i = 0; i < quests.Length; i++)
            {
                if (quests[i].quest != null) quests[i].name = quests[i].quest.name;
                else quests[i].name = "null";
            }
        }
#endif
    }
}