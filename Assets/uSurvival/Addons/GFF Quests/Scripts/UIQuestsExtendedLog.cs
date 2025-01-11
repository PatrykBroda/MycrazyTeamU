using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIQuestsExtendedLog : MonoCache
    {
        public UIQuestsExtended questsExtended;
        public GameObject panelQuests;

        [Header("Panel Quests log")]
        public GameObject panelQuestsLog;
        public Transform contentQuestsLog;
        public GameObject prefabQuestsLog;
        public Button buttonQuestsLogClose;
        public Button buttonQuestsLogAll;
        public Button buttonQuestsLogStory;
        public Button buttonQuestsLogDaily;
        public Button buttonQuestsLogGuild;
        public Button buttonQuestsLogPremium;

        public static bool questsLogOpen = false;
        public static bool allIsOn = true;
        public static bool storyIsOn = true;
        public static bool dailyIsOn = true;
        public static bool guildIsOn = true;
        public static bool premiumIsOn = true;

        private List<Quest> activeQuests = new List<Quest>();

        private void Start()
        {
            //load data from playerprefs to questsLog state
            if (questsExtended.saveStateQuestsLogToPlayerPrefs)
            {
                if (PlayerPrefs.HasKey("QuestsLogState"))
                {
                    if (PlayerPrefs.GetInt("QuestsLogState") == 1)
                    {
                        questsLogOpen = true;
                        questsExtended.toggleShowList.isOn = true;
                    }
                }
            }

            questsExtended.toggleShowList.gameObject.SetActive(questsExtended.useQuestsLog);
            questsExtended.toggleShowList.onValueChanged.AddListener(delegate { OpenCloseQuestslog(); });

            //buttons
            buttonQuestsLogClose.onClick.SetListener(() =>
            {
                panelQuestsLog.SetActive(false);
                questsExtended.toggleShowList.isOn = false;
                SaveState();
            });

            buttonQuestsLogAll.onClick.SetListener(() =>
            {
                buttonQuestsLogAll.GetComponentInChildren<Text>().color = Color.white;
                buttonQuestsLogStory.GetComponentInChildren<Text>().color = Color.white;
                buttonQuestsLogDaily.GetComponentInChildren<Text>().color = Color.white;
                buttonQuestsLogGuild.GetComponentInChildren<Text>().color = Color.white;
                buttonQuestsLogPremium.GetComponentInChildren<Text>().color = Color.white;

                allIsOn = true;
                storyIsOn = true;
                dailyIsOn = true;
                guildIsOn = true;
                premiumIsOn = true;
            });

            buttonQuestsLogStory.onClick.SetListener(() =>
            {
                if (storyIsOn == true)
                {
                    buttonQuestsLogStory.GetComponentInChildren<Text>().color = Color.grey;
                    buttonQuestsLogAll.GetComponentInChildren<Text>().color = Color.grey;
                }
                else buttonQuestsLogStory.GetComponentInChildren<Text>().color = Color.white;

                storyIsOn = !storyIsOn;
            });

            buttonQuestsLogDaily.onClick.SetListener(() =>
            {
                if (dailyIsOn)
                {
                    buttonQuestsLogDaily.GetComponentInChildren<Text>().color = Color.grey;
                    buttonQuestsLogAll.GetComponentInChildren<Text>().color = Color.grey;
                }
                else buttonQuestsLogDaily.GetComponentInChildren<Text>().color = Color.white;

                dailyIsOn = !dailyIsOn;
            });

            buttonQuestsLogGuild.onClick.SetListener(() =>
            {
                if (guildIsOn)
                {
                    buttonQuestsLogGuild.GetComponentInChildren<Text>().color = Color.grey;
                    buttonQuestsLogAll.GetComponentInChildren<Text>().color = Color.grey;
                }
                else buttonQuestsLogGuild.GetComponentInChildren<Text>().color = Color.white;

                guildIsOn = !guildIsOn;
            });
        }

        public override void OnTick()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                panelQuestsLog.SetActive(questsLogOpen);
                if (panelQuestsLog.activeSelf)
                {
                    buttonQuestsLogGuild.gameObject.SetActive(player.guild.InGuild());

                    //show guests
                    activeQuests.Clear();
                    for (int i = 0; i < player.quests.quests.Count; i++)
                    {
                        if (!player.quests.quests[i].completed &&
                            ((player.quests.quests[i].cooldown == 0 && !player.quests.quests[i].guild && storyIsOn) ||
                            (player.quests.quests[i].cooldown > 0 && !player.quests.quests[i].guild && dailyIsOn) ||
                            (player.quests.quests[i].guild && guildIsOn)))
                        {
                            activeQuests.Add(player.quests.quests[i]);
                        }
                    }

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(prefabQuestsLog.gameObject, activeQuests.Count, contentQuestsLog);

                    for (int i = 0; i < activeQuests.Count; i++)
                    {
                        UIQuestsLog slot = contentQuestsLog.GetChild(i).GetComponent<UIQuestsLog>();
                        Quest quest = activeQuests[i];

                        //tool tip
                        slot.tooltip.enabled = questsExtended.useToolTip;
                        slot.tooltip.text = quest.data.ToolTip(player, quest);

                        //color
                        slot.textType.color = quest.data.color;

                        //name
                        slot.textQuestName.text = Localization.Translate(quest.name);

                        //completed ?
                        slot.imagestate.gameObject.SetActive(quest.IsFulfilled(player));

                        // tasks
                        if (quest.data is KillQuest killQuest)
                        {
                            slot.textQuestTarget.text = "- Kill: " + killQuest.killTarget.name + " " + quest.progress + "/" + killQuest.killAmount;
                        }
                        else if (quest.data is LocationQuest)
                        {
                            slot.textQuestTarget.text = "- Inspect the Blocked Path";
                        }
                        else if (quest.data is GatherQuest gatherQuest)
                        {
                            string info = "";
                            for (int x = 0; x < gatherQuest.gatherItems.Length; x++)
                            {
                                if (gatherQuest.gatherItems[x].item != null)
                                {
                                    info = info + " - Gather: " + gatherQuest.gatherItems[x].item.name + " " + player.inventory.Count(new Item(gatherQuest.gatherItems[x].item)) + "/" + gatherQuest.gatherItems[x].amount + " \n";
                                }
                            }
                            slot.textQuestTarget.text = info;
                        }

                        //button
                        if (questsExtended.useButtonsInQuestslog)
                        {
                            slot.button.onClick.SetListener(() =>
                            {
                                panelQuests.SetActive(true);

                                if (quest.cooldown > 0 && !quest.guild)
                                {
                                    questsExtended.SetQuestToDaily();
                                    questsExtended.textQuestInfoType.text = Localization.Translate("Daily");
                                    FindQuestFromContent(slot.textQuestName.text);
                                }
                                else if (quest.guild)
                                {
                                    questsExtended.SetQuestToGuild();
                                    questsExtended.textQuestInfoType.text = Localization.Translate("Guild");
                                    UIQuestsExtended.selectedQuest = quest;
                                    FindQuestFromContent(slot.textQuestName.text);
                                }
                                else
                                {
                                    questsExtended.SetQuestToStory();
                                    questsExtended.textQuestInfoType.text = Localization.Translate("Story");
                                    UIQuestsExtended.selectedQuest = quest;
                                    FindQuestFromContent(slot.textQuestName.text);
                                }
                            });
                        }
                    }
                }
            }
            else panelQuestsLog.SetActive(false);
        }

        private void OpenCloseQuestslog()
        {
            questsLogOpen = questsExtended.toggleShowList.isOn;
            SaveState();
        }

        private void SaveState()
        {
            if (questsExtended.saveStateQuestsLogToPlayerPrefs)
                PlayerPrefs.SetInt("QuestsLogState", questsLogOpen.ToInt());
        }

        private void FindQuestFromContent(string questName)
        {
            for (int i = 0; i < questsExtended.contentQuests.childCount; i++)
            {
                if (questsExtended.contentQuests.GetChild(i).GetComponent<UIQuestPrefab>().textName.text == questName)
                {
                    questsExtended.contentQuests.GetChild(i).gameObject.GetComponent<Image>().color = Color.yellow;
                    break;
                }
            }
        }
    }
}