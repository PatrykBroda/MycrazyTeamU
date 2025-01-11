using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UINewQuest : MonoBehaviour
    {
        public GameObject panel;

        [Header("UI Elements : Quest info")]
        public Text textQuestInfoType;
        public Text textQuestInfoName;
        public Text textQuestInfoDescription;
        public Text textTask;
        public Text[] textRewardsValue;
        public Transform rewardItemsContent;
        public GameObject rewardItemsPrefab;

        public Button buttonCancel;
        public Button buttonAccept;

        [Header("Scripts")]
        public UIQuestsExtended questsExtended;

        void Update()
        {
            Player player = Player.localPlayer;
            if (player != null && player.quests.shareQuestRequest != "")
            {
                if (ScriptableQuest.dict.TryGetValue(player.quests.shareQuestRequest.GetStableHashCode(), out ScriptableQuest questData))
                {
                    Quest quest = new Quest(questData);
                    panel.SetActive(true);
                    ShowQuestInfo(player, quest);

                    buttonAccept.interactable = (player.quests.CanAcceptQuestExtended(quest.data));
                    buttonAccept.onClick.SetListener(() => { player.quests.CmdAcceptQuestFromPlayer(quest); });
                }

                buttonCancel.onClick.SetListener(() => { player.quests.CmdShareQuestRequestDecline(); });
            }
            else panel.SetActive(false);
        }

        private void ShowQuestInfo(Player player, Quest quest)
        {
            //type
            if (quest.cooldown > 0 && !quest.guild) textQuestInfoType.text = Localization.Translate("Daily");
            else if (quest.guild == true) textQuestInfoType.text = Localization.Translate("Guild");
            else textQuestInfoType.text = Localization.Translate("Story");

            //name & Description
            textQuestInfoName.text = Localization.Translate(quest.name);
            textQuestInfoDescription.text = quest.data.GetDescriptionByLanguage(Localization.languageCurrent);

            //tasks
            if (quest.data is KillQuest kQuest)
                textTask.text = "- " + Localization.Translate("Kill") + ": " + Localization.Translate(kQuest.killTarget.name) + " " + 0 + "/" + kQuest.killAmount;

            if (quest.data is LocationQuest)
                textTask.text = "- Inspect the Blocked Path";

            if (quest.data is GatherQuest gQuest)
            {
                string info = "";
                for (int i = 0; i < gQuest.gatherItems.Length; i++)
                {
                    if (gQuest.gatherItems[i].item != null)
                    {
                        Color tempColor;
                        if (player.inventory.Count(new Item(gQuest.gatherItems[i].item)) >= gQuest.gatherItems[i].amount) tempColor = questsExtended.colorDone;
                        else tempColor = questsExtended.colorNotDone;

                        info = info + "<color=#" + UtilsExtended.GetStringFromColor(tempColor) + "> - " + Localization.Translate("Gather") + ": " +
                            Localization.Translate(gQuest.gatherItems[i].item.name) + " " +
                            player.inventory.Count(new Item(gQuest.gatherItems[i].item)) + "/" +
                            gQuest.gatherItems[i].amount + "</color> \n";
                    }
                }
                textTask.text = info;
            }

            //rewards
            int ind = 0;
            for (int i = 0; i < textRewardsValue.Length; i++) textRewardsValue[i].text = "";

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
            if (quest.rewardItems.Length > 0)
            {
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(rewardItemsPrefab.gameObject, quest.rewardItems.Length, rewardItemsContent.transform);

                // refresh all items
                for (int i = 0; i < quest.rewardItems.Length; ++i)
                {
                    if (quest.rewardItems[i].item != null)
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

                        // addon system hooks (Item rarity)
                        //UtilsExtended.InvokeMany(typeof(UINewQuest), this, "Update_", slot, itemSlot);
                    }
                }
            }
            else
            {
                //remove all prefabs
                for (int i = 0; i < rewardItemsContent.transform.childCount; ++i) Destroy(rewardItemsContent.transform.GetChild(i).gameObject);
            }
        }
    }
}


