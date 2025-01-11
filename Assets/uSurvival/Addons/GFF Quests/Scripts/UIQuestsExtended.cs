using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIQuestsExtended : MonoBehaviour
    {
        public enum QuestType : byte { All, Story, Daily, Guild }

        [Header("Settings : sounds")]
        public SoundsSystem soundSystem;
        public AudioSource audioSource;
        public AudioClip soundQuestAccepted;
        public AudioClip soundCompleteQuest;

        [Header("Settings : Quests log")]
        public bool useQuestsLog;
        public bool saveStateQuestsLogToPlayerPrefs;
        //public int questsLogAmountLimit;
        public bool useToolTip;
        public bool useButtonsInQuestslog;

        [Header("Settings : Colors")]
        public Color colorNotDone = Color.white;
        public Color colorDone = Color.green;
        public Color colorSelectedReward = Color.yellow;
        public Color colorNotSelectedReward = Color.white;

        [Header("Quests on Player")]
        public Button buttonOpenPanelQuests;
        public GameObject panel;
        public Transform contentQuests;
        public UIQuestPrefab QuestPrefab;
        public Text textQuestvalue;
        public Toggle toggleShowList;
        public Dropdown dropdownShowingQuests;

        [Header("UI Elements : Quest info")]
        public Text textQuestInfoType;
        public Text textQuestInfoName;
        public Text textQuestInfoDescription;
        public Text textTask;
        public Text[] textRewardsValue;
        public Transform rewardItemsContent;
        public GameObject rewardItemsPrefab;

        public Button buttonCancel;
        public Button buttonShare;
        public Button buttonComplete;

        [Header("Panel : info")]
        public GameObject panelInfo;
        public Text textInfo;

        public static QuestType questType;
        private List<Quest> activeQuests = new List<Quest>();
        public static Quest selectedQuest;
        private int selectedQuestIndex = -1;

        [Header("Scripts")]
        public UIQuestsByNpc NpcQuestsScript;

        //singleton
        public static UIQuestsExtended singleton;
        public UIQuestsExtended() { singleton = this; }

        private void Start()
        {
            List<string> names = new List<string>();
            foreach (QuestType questType in Enum.GetValues(typeof(QuestType)))
            {
                names.Add(Localization.Translate(questType.ToString()));
            }
            dropdownShowingQuests.AddOptions(names);
            dropdownShowingQuests.onValueChanged.AddListener(delegate
            {
                questType = (QuestType)dropdownShowingQuests.value;
                selectedQuestIndex = -1;
            });

            buttonOpenPanelQuests.onClick.SetListener(() => { panel.SetActive(!panel.activeSelf); });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                if (panel.activeSelf)
                {
                    activeQuests.Clear();
                    if (questType == QuestType.All)
                    {
                        foreach (Quest quest in player.quests.quests)
                            if(!quest.completed)activeQuests.Add(quest);
                    }
                    else if (questType == QuestType.Story)
                    {
                        foreach (Quest quest in player.quests.quests)
                            if (!quest.completed && quest.cooldown == 0 && !quest.guild) activeQuests.Add(quest);
                    }
                    else if (questType == QuestType.Daily)
                    {
                        foreach (Quest quest in player.quests.quests)
                            if (!quest.completed && quest.cooldown > 0 && !quest.guild) activeQuests.Add(quest);
                    }
                    else if (questType == QuestType.Guild)
                    {
                        foreach (Quest quest in player.quests.quests)
                            if (!quest.completed && quest.guild) activeQuests.Add(quest);
                    }

                    if (selectedQuestIndex == -1 && activeQuests.Count > 0) selectedQuestIndex = 0;
                    else if (selectedQuestIndex != -1 && activeQuests.Count == 0) selectedQuestIndex = -1;

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(QuestPrefab.gameObject, activeQuests.Count, contentQuests);

                    // refresh all
                    for (int i = 0; i < activeQuests.Count; ++i)
                    {
                        UIQuestPrefab slot = contentQuests.GetChild(i).GetComponent<UIQuestPrefab>();
                        Quest quest = activeQuests[i];
                        slot.textName.text = Localization.Translate(quest.name);

                        //button
                        int icopy = i;
                        slot.button.onClick.SetListener(() =>
                        {
                            selectedQuest = quest;
                            selectedQuestIndex = icopy;
                        });

                        //status
                        slot.imageStatus.gameObject.SetActive(quest.IsFulfilled(player));

                        if (selectedQuestIndex == i) slot.GetComponent<Image>().color = Color.yellow;
                        else slot.GetComponent<Image>().color = Color.gray;
                    }

                    textQuestvalue.text = Localization.Translate("Accepted quests") + " " + player.quests.CountIncomplete() + "/" + player.quests.activeQuestLimit;

                    //show quest info
                    ShowQuestInfo(player);
                }
            }
            else panel.SetActive(false);
        }

        //choice of type of quests
        public void SetQuestToStory()
        {
            questType = QuestType.Story;
            selectedQuestIndex = -1;
        }
        public void SetQuestToDaily()
        {
            questType = QuestType.Daily;
            selectedQuestIndex = -1;
        }
        public void SetQuestToGuild()
        {
            questType = QuestType.Guild;
            selectedQuestIndex = -1;
        }

        private void ShowQuestInfo(Player player)
        {
            if (selectedQuestIndex != -1)
            {
                selectedQuest = activeQuests[selectedQuestIndex];
                textQuestInfoType.text = Localization.Translate(questType.ToString());
                textQuestInfoName.text = Localization.Translate(selectedQuest.name);
                textQuestInfoDescription.text = selectedQuest.data.GetDescriptionByLanguage(Localization.languageCurrent);

                //tasks
                if (selectedQuest.data is KillQuest kQuest)
                    textTask.text = "- " + Localization.Translate("Kill") + ": " + Localization.Translate(kQuest.killTarget.name) + " " + selectedQuest.progress + "/" + kQuest.killAmount;

                if (selectedQuest.data is LocationQuest)
                    textTask.text = selectedQuest.data.GetTaskByLanguage(Localization.languageCurrent);

                if (selectedQuest.data is GatherQuest gQuest)
                {
                    string info = "";
                    for (int i = 0; i < gQuest.gatherItems.Length; i++)
                    {
                        if (gQuest.gatherItems[i].item != null)
                        {
                            Color tempColor;
                            if (player.inventory.Count(new Item(gQuest.gatherItems[i].item)) >= gQuest.gatherItems[i].amount) tempColor = colorDone;
                            else tempColor = colorNotDone;

                            info = info + "<color=#" + UtilsExtended.GetStringFromColor(tempColor) + "> - " + Localization.Translate("Gather") + ": " + Localization.Translate(gQuest.gatherItems[i].item.name) + " " +
                                player.inventory.Count(new Item(gQuest.gatherItems[i].item)) + "/" + gQuest.gatherItems[i].amount + "</color> \n";
                        }
                    }
                    textTask.text = info;
                }

                //rewards
                int ind = 0;
                for (int i = 0; i < textRewardsValue.Length; i++) textRewardsValue[i].text = "";

                if (selectedQuest.rewardCoins > 0)
                {
                    textRewardsValue[ind].text = Localization.Translate("Coins") + ": " + selectedQuest.rewardCoins;
                    ind += 1;
                }
                if (selectedQuest.rewardGold > 0)
                {
                    textRewardsValue[ind].text = Localization.Translate("Gold") + ": " + selectedQuest.rewardGold;
                    ind += 1;
                }

                //rewards items
                int chooseReward = -1;
                if (selectedQuest.rewardItems.Length > 0)
                {
                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(rewardItemsPrefab.gameObject, selectedQuest.rewardItems.Length, rewardItemsContent.transform);

                    // refresh all items
                    for (int i = 0; i < selectedQuest.rewardItems.Length; ++i)
                    {
                        if (selectedQuest.rewardItems[i].item != null)
                        {
                            UniversalSlot slot = rewardItemsContent.transform.GetChild(i).GetComponent<UniversalSlot>();

                            ItemSlot itemSlot = new ItemSlot();
                            itemSlot.item = new Item(selectedQuest.rewardItems[i].item);
                            itemSlot.amount = selectedQuest.rewardItems[i].amount;

                            //ToolTip
                            slot.tooltip.enabled = true;
                            slot.tooltip.text = itemSlot.ToolTip();

                            slot.button.enabled = false;
                            slot.image.sprite = selectedQuest.rewardItems[i].item.image;
                            slot.amountOverlay.SetActive(selectedQuest.rewardItems[i].amount > 1);
                            slot.amountText.text = selectedQuest.rewardItems[i].amount.ToString();

                            // addon system hooks (Item rarity)
                            //UtilsExtended.InvokeMany(typeof(UIQuestsExtended), this, "Update_", slot, itemSlot);

                            int icopy = i;
                            slot.button.enabled = selectedQuest.data.chooseReward && player.quests.CanCompleteQuest(selectedQuest.name);
                            slot.button.onClick.SetListener(() =>
                            {
                                chooseReward = icopy;

                                buttonComplete.interactable = player.quests.CanCompleteQuest(selectedQuest.name) && (!selectedQuest.data.chooseReward || selectedQuest.data.chooseReward && chooseReward != -1);

                                //change color
                                for (int x = 0; x < rewardItemsContent.childCount; x++)
                                {
                                    rewardItemsContent.GetChild(x).gameObject.GetComponent<Image>().color = colorNotSelectedReward;
                                }
                                rewardItemsContent.GetChild(chooseReward).gameObject.GetComponent<Image>().color = colorSelectedReward;
                            });
                        }
                    }
                }
                else
                {
                    //remove all prefabs
                    for (int i = 0; i < rewardItemsContent.transform.childCount; ++i) Destroy(rewardItemsContent.transform.GetChild(i).gameObject);
                }

                //buttons in Quest info
                buttonCancel.gameObject.SetActive(selectedQuest.data.isCanceled);
                buttonCancel.interactable = true;
                buttonCancel.onClick.SetListener(() =>
                {
                    player.quests.CmdCancelQuestExtended(selectedQuest);
                    ClearQuestInfo();
                });

                /*buttonShare.gameObject.SetActive(selectedQuest.data.isShare);
                buttonShare.interactable = (player.InParty());
                buttonShare.onClick.SetListener(() => {
                    if (player.InParty()) player.CmdShareQuestRequestSend(selectedQuest);
                });*/

                buttonComplete.gameObject.SetActive(selectedQuest.data.completeImmediately);
                buttonComplete.onClick.SetListener(() =>
                {
                    bool hasSpace = selectedQuest.rewardItems.Length == 0 ||
                                    !selectedQuest.data.chooseReward && player.inventory.SlotsFree() >= selectedQuest.rewardItems.Length ||
                                    selectedQuest.data.chooseReward && player.inventory.CanAdd(new Item(selectedQuest.rewardItems[chooseReward].item), selectedQuest.rewardItems[chooseReward].amount);

                    // description + not enough space warning (if needed)
                    if (!hasSpace)
                    {
                        textInfo.text = "<color=red>Not enough inventory space!</color>";
                        panelInfo.SetActive(true);
                    }
                    else
                    {
                        player.quests.CmdCompleteQuestImmediately(selectedQuest, chooseReward);
                        ClearQuestInfo();
                    }
                });
            }
            else ClearQuestInfo();
        }

        private void ClearQuestInfo()
        {
            selectedQuest = new Quest();

            //name
            textQuestInfoType.text = "";
            textQuestInfoName.text = "";
            textQuestInfoDescription.text = "";

            //tasks
            textTask.text = "";

            //rewards
            for (int i = 0; i < textRewardsValue.Length; i++) textRewardsValue[i].text = "";

            //rewards items
            for (int i = 0; i < rewardItemsContent.transform.childCount; ++i) Destroy(rewardItemsContent.transform.GetChild(i).gameObject);

            buttonCancel.gameObject.SetActive(false);
            buttonShare.gameObject.SetActive(false);
            buttonComplete.gameObject.SetActive(false);
        }

        //sounds
        public void PlaySoundQuestAccepted()
        {
            if (soundSystem == SoundsSystem.viaIndividualSounds) audioSource.PlayOneShot(soundQuestAccepted);
            else if (soundSystem == SoundsSystem.viaAddonMenu)
            {
                // addon system hooks (Menu Sounds)
                UtilsExtended.InvokeMany(typeof(UIQuestsExtended), this, "PlaySoundUI_", soundQuestAccepted);
            }
        }
    }
}