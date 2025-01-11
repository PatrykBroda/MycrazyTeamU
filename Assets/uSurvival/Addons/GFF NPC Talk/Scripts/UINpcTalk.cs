using System;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;
using System.Collections.Generic;

namespace GFFAddons
{
    public class UINpcTalk : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonBack;

        //panel dialogues
        [SerializeField] private GameObject panelDialogues;
        [SerializeField] private Text textDialogueWelcome;
        [SerializeField] private Transform continuedDialogueContent;
        [SerializeField] private GameObject dialoguePrefab;

        [Header("Dialogue info")]
        [SerializeField] private GameObject panelDialogueInfo;
        [SerializeField] private Text textDialogueInfo;

        [Header("Quests")]
        [SerializeField] private GameObject panelQuestInfo;
        [SerializeField] private Text textQuestInfoType;
        [SerializeField] private Text textQuestInfoName;
        [SerializeField] private Text textQuestInfoDescription;
        [SerializeField] private Text textTask;
        [SerializeField] private Text[] textRewardsValue;
        [SerializeField] private Transform rewardItemsContent;
        [SerializeField] private GameObject rewardItemsPrefab;
        [SerializeField] private Button buttonQuestAction;
        public Color colorSelectedReward = Color.yellow;
        public Color colorNotSelectedReward = Color.white;

        [Header("Quest info")]
        public GameObject panelInfo;
        public Text textInfo;

        [Header("Components")]
        [SerializeField] private AudioSource audioSource;

        private ScriptableDialogueData talkData;

        private ScriptableQuest selectedQuest = null;
        private int chooseReward = -1;

        public static UINpcTalk singleton;
        public UINpcTalk()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            //if (singleton == null) 
            singleton = this;
        }

        private void Start()
        {
            buttonClose.onClick.SetListener(() =>
            {
                audioSource.Play();
                if (panelDialogueInfo.activeSelf || panelQuestInfo.activeSelf)
                {
                    panelDialogueInfo.SetActive(false);
                    panelQuestInfo.gameObject.SetActive(false);
                    panelDialogues.SetActive(true);
                }
                else
                {
                    panel.SetActive(false);
                }
            });

            buttonBack.onClick.SetListener(() =>
            {
                audioSource.Play();

                if (panelDialogueInfo.activeSelf) panelDialogueInfo.SetActive(false);
                else talkData = talkData.predecessor;
            });
        }

        public void Show(ScriptableDialogueData data)
        {
            talkData = data;
            panel.SetActive(true);
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                if (talkData != null)
                {
                    Player player = Player.localPlayer;
                    if (player != null)
                    {
                        buttonQuestAction.gameObject.SetActive(panelQuestInfo.activeSelf);

                        buttonBack.gameObject.SetActive(!panelQuestInfo.activeSelf && talkData != null && talkData.predecessor != null);

                        List<NpcDialogue> interactions = talkData.GetInteractions(player, Localization.languageCurrent);

                        if (panelDialogues.activeSelf)
                        {
                            textDialogueWelcome.text = talkData.GetWelcomeTextByLanguage(Localization.languageCurrent);

                            // instantiate/destroy enough slots
                            UIUtils.BalancePrefabs(dialoguePrefab, interactions.Count, continuedDialogueContent);

                            for (int i = 0; i < interactions.Count; i++)
                            {
                                UINpcTalkInteractionSlot slot = continuedDialogueContent.GetChild(i).GetComponent<UINpcTalkInteractionSlot>();

                                slot.textName.text = interactions[i].name;

                                //if this is a quest ?
                                if (interactions[i].quest != null)
                                {
                                    slot.imageInteract.gameObject.SetActive(true);
                                    slot.imageInteract.color = player.quests.CanAcceptQuestExtended(interactions[i].quest) || player.quests.CanCompleteQuest(interactions[i].quest.name) ? colorSelectedReward : Color.white;
                                }
                                else slot.imageInteract.gameObject.SetActive(false);

                                int icopy = i;
                                slot.button.onClick.SetListener(() =>
                                {
                                    audioSource.Play();

                                    if (interactions[icopy].quest != null)
                                    {
                                        selectedQuest = interactions[icopy].quest;
                                        panelQuestInfo.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        if (interactions[icopy].continuedDialogue != null) talkData = interactions[icopy].continuedDialogue;
                                        else
                                        {
                                            textDialogueInfo.text = interactions[icopy].GetDialogueLocalization(Localization.languageCurrent);
                                            panelDialogueInfo.SetActive(true);
                                        }
                                    }
                                });
                            }
                        }

                        if (panelQuestInfo.activeSelf)
                        {
                            if (selectedQuest == null)
                            {
                                panelQuestInfo.SetActive(false);
                                return;
                            }

                            Quest quest = new Quest(selectedQuest);

                            //type
                            if (quest.cooldown == 0 && !quest.guild) textQuestInfoType.text = Localization.Translate("Story");
                            else if (quest.cooldown > 0 && !quest.guild) textQuestInfoType.text = Localization.Translate("Daily");
                            else if (quest.cooldown == 0 && quest.guild) textQuestInfoType.text = Localization.Translate("Guild");

                            //name
                            textQuestInfoName.text = Localization.Translate(quest.name);

                            //description
                            textQuestInfoDescription.text = quest.data.description;

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
                                slot.tooltipExtended.enabled = true;
                                if (slot.tooltipExtended.IsVisible())
                                    slot.tooltipExtended.slot = itemSlot;

                                slot.image.sprite = quest.rewardItems[i].item.image;
                                slot.amountOverlay.SetActive(quest.rewardItems[i].amount > 1);
                                slot.amountText.text = quest.rewardItems[i].amount.ToString();

                                //show color border
                                if (chooseReward == i) slot.GetComponent<Image>().color = colorSelectedReward;
                                else slot.GetComponent<Image>().color = colorNotSelectedReward;

                                int icopy = i;
                                slot.button.enabled = quest.data.chooseReward && canCompleteQuest;
                                slot.button.onClick.SetListener(() =>
                                {
                                    chooseReward = icopy;
                                });
                            }

                            if (player.quests.CanAcceptQuestExtended(quest.data))
                            {
                                //update rewards items
                                chooseReward = -1;

                                // new quest
                                buttonQuestAction.GetComponentInChildren<Text>().text = Localization.Translate("Accept");
                                buttonQuestAction.interactable = true;
                                buttonQuestAction.onClick.SetListener(() =>
                                {
                                    selectedQuest = null;
                                    //PlaySoundQuestAccepted();

                                    player.quests.CmdAcceptQuest(quest.name);
                                    panelQuestInfo.gameObject.SetActive(false);
                                });
                            }
                            else
                            {
                                buttonQuestAction.GetComponentInChildren<Text>().text = Localization.Translate("Complete");
                                buttonQuestAction.interactable = canCompleteQuest && (!quest.data.chooseReward || chooseReward != -1);
                                buttonQuestAction.onClick.SetListener(() =>
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
                                        player.quests.CmdCompleteQuestImmediately(quest, chooseReward);
                                        chooseReward = -1;
                                    }

                                    selectedQuest = null;
                                    panelQuestInfo.SetActive(false);
                                });
                            }
                        }
                    }
                    else panel.SetActive(false);
                }
                else panel.SetActive(false);
            }
        }
    }
}