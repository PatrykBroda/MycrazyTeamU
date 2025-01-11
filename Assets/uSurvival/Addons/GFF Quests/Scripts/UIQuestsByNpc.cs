using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIQuestsByNpc : MonoBehaviour
    {
        [Header("Components")]
        public UIQuestsExtended questsExtended;

        public GameObject panel;
        public Transform content;
        public UIQuestPrefab QuestPrefab;
        public Text textQuestsValue;

        [Header("Panel : Quest Info")]
        public Text textQuestInfoType;
        public Text textQuestInfoName;
        public Text textQuestInfoDescription;
        public Text textTask;
        public Text[] textRewardsValue;
        public Transform rewardItemsContent;
        public GameObject rewardItemsPrefab;
        public Button actionButton;

        [Header("Panel : info")]
        public GameObject panelInfo;
        public Text textInfo;

        //private List<ScriptableQuest> questsAvailable;
        private int selectedQuest = 0;
        private int chooseReward = -1;
        private string selectedQuestName = "";

        //singleton
        public static UIQuestsByNpc singleton;
        public UIQuestsByNpc()
        {
            singleton = this;
        }

        public void Show()
        {
            panel.SetActive(true);  
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null && player.health.current > 0 &&
                    player.interactionExtended.target != null && player.interactionExtended.target is Npc npc &&
                    Utils.ClosestDistance(player.collider, npc.collider) <= player.interactionRange)
                {
                    List<ScriptableQuest> questsAvailable = npc.quests.QuestsVisibleFor(player);
                    if (questsAvailable.Count == 0) panel.SetActive(false);
                    if (selectedQuest > questsAvailable.Count - 1 || selectedQuestName != questsAvailable[selectedQuest].name)
                    {
                        selectedQuest = 0;
                        selectedQuestName = questsAvailable[selectedQuest].name;
                    }

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(QuestPrefab.gameObject, questsAvailable.Count, content);

                    // refresh all
                    for (int i = 0; i < questsAvailable.Count; ++i)
                    {
                        UIQuestPrefab slot = content.GetChild(i).GetComponent<UIQuestPrefab>();
                        slot.textName.text = Localization.Translate(questsAvailable[i].name);

                        // find quest index in player quest list
                        int questIndex = player.quests.GetIndexByName(questsAvailable[i].name);

                        //status image
                        slot.imageStatus.gameObject.SetActive(questIndex != -1 && player.quests.quests[questIndex].IsFulfilled(player));

                        //show color border
                        if (selectedQuest == i) slot.GetComponent<Image>().color = Color.yellow;
                        else slot.GetComponent<Image>().color = Color.gray;

                        //button
                        int icopy = i;
                        slot.button.onClick.SetListener(() =>
                        {
                            selectedQuest = icopy;
                            selectedQuestName = questsAvailable[icopy].name;
                            chooseReward = -1;
                        });
                    }

                    textQuestsValue.text = Localization.Translate("Accepted quests") + ": " + player.quests.CountIncomplete() + "/" + player.quests.activeQuestLimit;

                    if (selectedQuest != -1 && questsAvailable.Count > selectedQuest)
                    {
                        Quest quest = new Quest(questsAvailable[selectedQuest]);

                        //type
                        if (quest.cooldown == 0 && !quest.guild) textQuestInfoType.text = Localization.Translate("Story");
                        else if (quest.cooldown > 0 && !quest.guild) textQuestInfoType.text = Localization.Translate("Daily");
                        else if (quest.cooldown == 0 && quest.guild) textQuestInfoType.text = Localization.Translate("Guild");

                        //name
                        textQuestInfoName.text = Localization.Translate(quest.name);

                        //description
                        textQuestInfoDescription.text = quest.data.GetDescriptionByLanguage(Localization.languageCurrent);

                        //tasks
                        if (quest.data is KillQuest kQuest)
                            textTask.text = "- " + Localization.Translate("Kill") + ": " + Localization.Translate(kQuest.killTarget.name) + " " + quest.progress + "/" + kQuest.killAmount;

                        if (quest.data is LocationQuest)
                            textTask.text = "- -";

                        if (quest.data is GatherQuest gQuest)
                        {
                            string info = "";
                            for (int i = 0; i < gQuest.gatherItems.Length; i++)
                            {
                                if (gQuest.gatherItems[i].item != null)
                                    info = info + "- " + Localization.Translate("Gather") + ": " + Localization.Translate(gQuest.gatherItems[i].item.name) + " " + player.inventory.Count(new Item(gQuest.gatherItems[i].item)) + " /" + gQuest.gatherItems[i].amount + "\n";
                            }
                            textTask.text = info;
                        }

                        //rewards
                        for (int i = 0; i < textRewardsValue.Length; i++) textRewardsValue[i].text = "";

                        int ind = 0;
                        if (quest.rewardCoins > 0)
                        {
                            textRewardsValue[ind].text = Localization.Translate("Coins") + ": " + quest.rewardCoins;
                            ind += 1;
                        }
                        if (quest.rewardGold > 0)
                        {
                            textRewardsValue[ind].text = Localization.Translate("Gold") + ": " + quest.rewardGold;
                            ind += 1;
                        }

                        //rewards items
                        bool canCompleteQuest = player.quests.CanCompleteQuest(quest.name);
                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(rewardItemsPrefab.gameObject, quest.rewardItems.Length, rewardItemsContent.transform);

                        // refresh all items
                        for (int i = 0; i < quest.rewardItems.Length; ++i)
                        {
                            UniversalSlot slot = rewardItemsContent.transform.GetChild(i).GetComponent<UniversalSlot>();

                            ItemSlot itemSlot = new ItemSlot();
                            itemSlot.item = new Item(quest.rewardItems[i].item);
                            itemSlot.amount = quest.rewardItems[i].amount;

                            //ToolTip
                            slot.tooltip.enabled = true;
                            slot.tooltip.text = itemSlot.ToolTip();

                            slot.image.sprite = quest.rewardItems[i].item.image;
                            slot.amountOverlay.SetActive(quest.rewardItems[i].amount > 1);
                            slot.amountText.text = quest.rewardItems[i].amount.ToString();

                            //show color border
                            if (chooseReward == i) slot.GetComponent<Image>().color = questsExtended.colorSelectedReward;
                            else slot.GetComponent<Image>().color = questsExtended.colorNotSelectedReward;

                            // addon system hooks (Item rarity, upgrade)
                            //UtilsExtended.InvokeMany(typeof(UIQuestsByNpc), this, "Update_", player, slot, itemSlot);

                            int icopy = i;
                            slot.button.enabled = quest.data.chooseReward && canCompleteQuest;
                            slot.button.onClick.SetListener(() =>
                            {
                                chooseReward = icopy;
                            });
                        }

                        //buttons
                        if (player.quests.CanAcceptQuestExtended(quest.data))
                        {
                            //update rewards items
                            chooseReward = -1;

                            // new quest
                            actionButton.GetComponentInChildren<Text>().text = Localization.Translate("Accept");
                            actionButton.interactable = true;
                            actionButton.onClick.SetListener(() =>
                            {
                                selectedQuest = 0;
                                questsExtended.PlaySoundQuestAccepted();

                                // find quest index in original npc quest list (unfiltered)
                                int npcIndex = Array.FindIndex(((Npc)player.interactionExtended.target).quests.quests, entry => entry.quest.name == quest.name);

                                player.quests.CmdAcceptQuest(npcIndex);
                            });
                        }
                        else
                        {
                            actionButton.GetComponentInChildren<Text>().text = Localization.Translate("Complete");
                            actionButton.interactable = canCompleteQuest && (!quest.data.chooseReward || chooseReward != -1);
                            actionButton.onClick.SetListener(() =>
                            {
                                bool hasSpace = quest.rewardItems.Length == 0 ||
                                                !quest.data.chooseReward && player.inventory.SlotsFree() >= quest.rewardItems.Length ||
                                                quest.data.chooseReward && player.inventory.CanAdd(new Item(quest.rewardItems[chooseReward].item), quest.rewardItems[chooseReward].amount);

                                // description + not enough space warning (if needed)
                                if (!hasSpace)
                                {
                                    textInfo.text = "<color=red>Not enough inventory space!</color>";
                                    panelInfo.SetActive(true);
                                }
                                else
                                {
                                    // find quest index in original npc quest list (unfiltered)
                                    int npcIndex = Array.FindIndex(((Npc)player.interactionExtended.target).quests.quests, entry => entry.quest.name == quest.name);

                                    player.quests.CmdCompleteQuestExtended(npcIndex, chooseReward);
                                    chooseReward = -1;
                                }

                                selectedQuest = 0;
                            });
                        }
                    }
                }
                else panel.SetActive(false);
            }
        }
    }
}