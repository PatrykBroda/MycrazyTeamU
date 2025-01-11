using Mirror;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIRegistrationItemOnAuction : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UniversalSlot slot;
        [SerializeField] private Text textInfo;
        [SerializeField] private Text textItemName;
        [SerializeField] private InputField inputFieldAmount;
        [SerializeField] private InputField inputFieldPrice;
        [SerializeField] private Button buttonRegistration;
        [SerializeField] private Button buttonRegistrationCancel;

        [Header("If used Upgrade Addon")]
        [SerializeField] private Transform runesContent;
        [SerializeField] private GameObject runesPrefab;

        [Header("Components")]
        [SerializeField] private UIAuctionShowMyItems myItems;

        private double infoTimeEnd;

        public void Show()
        {
            panel.SetActive(true);
            UIMainPanel.singleton.panel.SetActive(true);
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    ItemSlot auctionSlot = new ItemSlot();

                    if (player.auction.registrationIndex != -1 && player.auction.registrationIndex < player.inventory.slots.Count && player.inventory.slots[player.auction.registrationIndex].amount > 0)
                    {
                        auctionSlot = player.inventory.slots[player.auction.registrationIndex];

                        slot.button.onClick.SetListener(() =>
                        {
                            player.auction.registrationIndex = -1;
                            player.auction.registrationIndexOld = -1;
                        });

                        // refresh valid item
                        slot.tooltip.enabled = true;
                        slot.tooltip.text = auctionSlot.ToolTip();
                        slot.dragAndDropable.dragable = true;
                        slot.image.color = Color.white;
                        slot.image.sprite = auctionSlot.item.image;
                        slot.amountOverlay.SetActive(auctionSlot.amount > 1);
                        slot.amountText.text = auctionSlot.amount.ToString();
                        textItemName.text = auctionSlot.item.name;

                        //if upgrade addon is used
                        //if (player.auction.useUpgradeAddon && player.auction.upgradeAddonPaintRunes)
                        //{
                        //    //if weapon or armor paint what runes enchanted item
                        //    if (auctionSlot.item.data is EquipmentItem)
                        //    {
                        //        int amount = 0;
                        //        if (player.auction.upgradeAddonUseHoles) amount = auctionSlot.item.holes;
                        //        else amount = auctionSlot.item.upgradeInd != null && auctionSlot.item.upgradeInd.Length > 0 ? auctionSlot.item.upgradeInd.Length : 0;

                        //        //instantiate
                        //        UIUtils.BalancePrefabs(runesPrefab, amount, runesContent);

                        //        //null all runes
                        //        for (int x = 0; x < amount; x++)
                        //        {
                        //            runesContent.GetChild(x).GetComponent<Image>().sprite = UIEnchantment.singleton.spriteRuneNull;
                        //            runesContent.GetChild(x).GetComponent<Image>().color = Color.gray;
                        //        }

                        //        //fill runes images
                        //        if (amount > 0)
                        //        {
                        //            for (int x = 0; x < auctionSlot.item.upgradeInd.Length; x++)
                        //            {
                        //                EnchantmentRuneItem item = EnchantmentRuneItem.GetRuneFromDictByType(auctionSlot.item.upgradeInd[x]);

                        //                runesContent.GetChild(x).GetComponent<Image>().sprite = item.image;
                        //                runesContent.GetChild(x).GetComponent<Image>().color = Color.white;
                        //            }
                        //        }
                        //    }
                        //}

                        // Enable\Disable InputField Amount
                        if (auctionSlot.amount > 1)
                        {
                            if (player.auction.registrationIndexOld != player.auction.registrationIndex)
                            {
                                inputFieldAmount.text = auctionSlot.amount.ToString();
                                inputFieldAmount.interactable = true;
                                inputFieldAmount.ActivateInputField();
                                player.auction.registrationIndexOld = player.auction.registrationIndex;

                                inputFieldAmount.onEndEdit.AddListener(delegate
                                {
                                    if (player.inventory.slots[player.auction.registrationIndex].amount < inputFieldAmount.text.ToInt())
                                        inputFieldAmount.text = player.inventory.slots[player.auction.registrationIndex].amount.ToString();
                                });
                            }

                            if (inputFieldAmount.text == "") inputFieldAmount.text = "1";
                            else if (int.Parse(inputFieldAmount.text) > auctionSlot.amount) inputFieldAmount.text = auctionSlot.amount.ToString();
                        }
                        else
                        {
                            inputFieldAmount.text = "1";
                            inputFieldAmount.interactable = false;
                            inputFieldPrice.ActivateInputField();
                        }

                        inputFieldPrice.interactable = true;
                    }
                    else
                    {
                        // refresh invalid item
                        slot.button.onClick.RemoveAllListeners();
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.amountOverlay.SetActive(false);

                        textItemName.text = "";
                        inputFieldAmount.text = "";
                        inputFieldPrice.text = "";
                        inputFieldAmount.interactable = false;
                        inputFieldPrice.interactable = false;

                        //clear indicators how enchanted item if used upgrade addon
                        //if (player.auction.useUpgradeAddon)
                        //{
                        //    for (int y = 0; y < runesContent.childCount; y++)
                        //    {
                        //        runesContent.GetChild(y).GetComponent<Image>().sprite = UIEnchantment.singleton.spriteRuneNull;
                        //        runesContent.GetChild(y).GetComponent<Image>().color = Color.clear;
                        //    }
                        //}
                    }

                    // addon system hooks (Item rarity, Upgrade)
                    UtilsExtended.InvokeMany(typeof(UIRegistrationItemOnAuction), this, "Update_", player, slot, auctionSlot);

                    // Enable\Disable Button registration Item
                    buttonRegistration.interactable = (player.auction.registrationIndex != -1 &&
                        inputFieldAmount.text != "" &&
                        !string.IsNullOrWhiteSpace(inputFieldPrice.text) &&
                        inputFieldPrice.text.ToLong() > 0);

                    buttonRegistration.onClick.SetListener(() =>
                    {
                        //enough Gold for registration ? 
                        if (player.auction.GetRegistrationTax() <= player.gold)
                        {
                            //send message to server
                            player.auction.CmdRegistrationNewItem(player.auction.registrationIndex, ushort.Parse(inputFieldAmount.text), uint.Parse(inputFieldPrice.text));

                            player.auction.registrationIndex = -1;
                            player.auction.registrationIndexOld = -1;
                            inputFieldAmount.text = "0";
                            inputFieldPrice.text = "0";
                        }
                        else
                        {
                            textInfo.text = "Registration for the auction costs " + player.auction.GetRegistrationTax() + " gold";
                            infoTimeEnd = NetworkTime.time + player.auction.infoMessageTime;
                        }
                    });

                    //Button registration cancel
                    buttonRegistrationCancel.onClick.SetListener(() =>
                    {
                        player.auction.registrationIndex = -1;
                        player.auction.registrationIndexOld = -1;

                        UIMainPanel.singleton.panel.SetActive(false);
                        panel.SetActive(false);
                        myItems.Show();
                    });

                    if (NetworkTime.time >= infoTimeEnd) textInfo.text = "";
                }
                else panel.SetActive(false);
            }
        }

        //click from ui Button
        public void CloseAuctionRegistration()
        {
            Player.localPlayer.auction.registrationIndex = -1;
        }
    }
}