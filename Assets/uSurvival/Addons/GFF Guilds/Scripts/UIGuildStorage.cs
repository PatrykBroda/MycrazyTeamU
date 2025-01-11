using Mirror;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public partial class UIGuildStorage : MonoBehaviour
    {
        [Header("Settings")]
        public KeyCode hotKeyConfirmAmount = KeyCode.KeypadEnter;

        [Header("Info messages")]
        public float timeForInfoText = 2.0f;
        private double timeInfo;

        [Header("Panels")]
        public GameObject panelMain;
        public GameObject panelStorageBuy;
        public GameObject panelPermissionDenied;

        [Header("For Panel Storage Buy")]
        public Text textGoldPrice;
        public Text textGoldInInventory;
        public Button buttonBuySlotsConfirm;
        public Button buttonBuySlotsCancel;
        public Text textInfo;

        [Header("For Panel Storage")]
        [SerializeField] private UniversalSlot slotPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private Text textStorageCount;
        [SerializeField] private GameObject panelGoldInStorage;
        [SerializeField] private TextMeshProUGUI textGoldInStorage;
        [SerializeField] private InputField inputFieldGold;
        [SerializeField] private Button buttonPlus;
        [SerializeField] private Button buttonMinus;
        [SerializeField] private Button buttonConfirmChangeGold;
        [SerializeField] private Button buttonStorageExpand;

        private enum State { goldToStorage, goldFromStorage }
        private State state;

        //singleton
        public static UIGuildStorage singleton;
        public UIGuildStorage() { singleton = this; }

        public void Show(Player player)
        {
            if (player.guild.InGuild())
            {
                //if the store has not yet been purchased
                if (player.guild.guild.storageSize == 0)
                {
                    panelStorageBuy.SetActive(true);
                }

                panelMain.SetActive(true);
            }
        }

        private void Update()
        {
            if (panelMain.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null && player.guild.InGuild() &&
                    player.interactionExtended.target != null &&
                    player.interactionExtended.target is Npc &&
                    uSurvival.Utils.ClosestDistance(player.collider, player.interactionExtended.target.collider) <= player.interactionRange)
                {
                    if (panelStorageBuy.activeSelf)
                    {
                        int goldRequiredToBuy = player.storageGuild.data.GetStoragePurchaseCost(player.guild.guild);
                        textGoldPrice.text = goldRequiredToBuy == 0 ? "0" : goldRequiredToBuy.ToString("###,###,###,###");
                        textGoldInInventory.text = player.gold == 0 ? "0" : player.gold.ToString("###,###,###,###");

                        buttonBuySlotsConfirm.interactable = player.storageGuild.CanBuySlots();
                        buttonBuySlotsConfirm.onClick.SetListener(() =>
                        {
                            buttonBuySlotsConfirm.interactable = false;
                            textInfo.gameObject.SetActive(true);
                            player.storageGuild.CmdGuildStorageBuy();
                            timeInfo = NetworkTime.time + timeForInfoText;
                        });

                        //view info text
                        if (timeInfo != 0 && timeInfo <= NetworkTime.time)
                        {
                            timeInfo = 0;
                            textInfo.gameObject.SetActive(false);
                            buttonBuySlotsConfirm.interactable = true;
                            panelStorageBuy.SetActive(false);
                        }
                    }

                    //textStorageCount.text = "Slots : " + player.guild.guild.storageSize + "/" + player.storageGuild.data.GetMaxSlotsAmountForStorage();

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(slotPrefab.gameObject, player.guild.guild.storageSize, content);

                    // refresh all items
                    for (int i = 0; i < player.guild.guild.storageSize; ++i)
                    {
                        UniversalSlot slot = content.GetChild(i).GetComponent<UniversalSlot>();
                        slot.dragAndDropable.name = i.ToString(); // drag and drop index
                        ItemSlot storageSlot = player.guild.guild.slots[i];

                        if (storageSlot.amount > 0)
                        {
                            // refresh valid item
                            int icopy = i; // needed for lambdas, otherwise i is Count
                            slot.button.onClick.SetListener(() =>
                            {
                                //transfer the item to inventory
                                player.storage.CmdTransferItemFromStorage(icopy);
                            });

                            //tooltip
                            //slot.tooltip.enabled = true;
                            //if (slot.tooltip.IsVisible())
                            //    slot.tooltip.text = storageSlot.ToolTip();

                            slot.tooltipExtended.enabled = true;
                            if (slot.tooltipExtended.IsVisible())
                                slot.tooltipExtended.slot = storageSlot;

                            slot.dragAndDropable.dragable = true;
                            slot.image.color = Color.white;
                            slot.image.sprite = storageSlot.item.image;

                            //amount
                            slot.amountOverlay.SetActive(storageSlot.amount > 1);
                            slot.amountText.text = storageSlot.amount.ToString();

                            slot.textDurability.text = storageSlot.item.durability > 1 ? ((int)(storageSlot.item.DurabilityPercent() * 100)).ToString() + "%" : "";
                            slot.imageBinding.SetActive(storageSlot.item.binding);
                        }
                        else
                        {
                            // refresh invalid item
                            slot.button.onClick.RemoveAllListeners();
                            slot.tooltip.enabled = false;
                            slot.tooltipExtended.enabled = false;
                            slot.dragAndDropable.dragable = false;
                            slot.image.color = Color.clear;
                            slot.image.sprite = null;
                            slot.amountOverlay.SetActive(false);

                            slot.sliderDurability.gameObject.SetActive(false);
                            slot.imageBinding.SetActive(false);
                            slot.textDurability.text = "";
                        }

                        // addon system hooks (Item rarity)
                        UtilsExtended.InvokeMany(typeof(UIGuildStorage), this, "Update_", player, slot, storageSlot);
                    }

                    // gold
                    textGoldInStorage.text = player.guild.guild.goldInStorage == 0 ? "0" : player.guild.guild.goldInStorage.ToString("###,###,###,###");

                    //Put gold to storage
                    buttonPlus.interactable = player.storageGuild.CanPutGoldToStorage();
                    buttonPlus.onClick.SetListener(() =>
                    {
                        state = State.goldToStorage;
                        buttonConfirmChangeGold.gameObject.SetActive(true);
                        buttonPlus.gameObject.SetActive(false);
                        buttonMinus.gameObject.SetActive(false);

                        inputFieldGold.text = "0";
                        inputFieldGold.gameObject.SetActive(true);
                        inputFieldGold.ActivateInputField();
                    });

                    //Take gold from storage
                    buttonMinus.interactable = player.storageGuild.CanTakeGoldFromStorage();
                    buttonMinus.onClick.SetListener(() =>
                    {
                        state = State.goldFromStorage;
                        buttonConfirmChangeGold.gameObject.SetActive(true);
                        buttonPlus.gameObject.SetActive(false);
                        buttonMinus.gameObject.SetActive(false);

                        inputFieldGold.text = "0";
                        inputFieldGold.gameObject.SetActive(true);
                        inputFieldGold.ActivateInputField();
                    });

                    //if we put in storage
                    if (string.IsNullOrEmpty(inputFieldGold.text) == false)
                    {
                        long goldInInputField = inputFieldGold.text.ToLong();
                        if (goldInInputField < 0)
                        {
                            goldInInputField = 0;
                            inputFieldGold.text = "0";
                        }

                        if (state == State.goldToStorage)
                        {
                            //if indicated more than available
                            if (inputFieldGold.text.ToLong() > player.gold)
                            {
                                inputFieldGold.text = player.gold.ToString();
                                goldInInputField = player.gold;
                            }

                            //if it is indicated more than it is possible to place
                            if (goldInInputField + player.guild.guild.goldInStorage > player.storageGuild.data.maxGoldInStorage)
                            {
                                inputFieldGold.text = (player.storageGuild.data.maxGoldInStorage - player.guild.guild.goldInStorage).ToString();
                            }
                        }
                        //if take away from storage
                        else
                        {
                            if (goldInInputField > player.guild.guild.goldInStorage) inputFieldGold.text = player.guild.guild.goldInStorage.ToString();
                        }
                    }

                    //if the player has confirmed the transfer of gold
                    if (Input.GetKeyDown(hotKeyConfirmAmount) && !UIUtils.AnyInputActive()) GoldExchangeConfirmation(player);
                    buttonConfirmChangeGold.onClick.SetListener(() => { GoldExchangeConfirmation(player); });

                    //add slots to storage
                    buttonStorageExpand.interactable = player.storageGuild.data.CanBuyMoreSlots(player.guild.guild);
                    buttonStorageExpand.onClick.SetListener(() =>
                    {
                        panelStorageBuy.SetActive(true);
                    });
                }
                else
                {
                    panelMain.SetActive(false);
                    panelPermissionDenied.SetActive(true);
                }
            }
        }

        private void GoldExchangeConfirmation(Player player)
        {
            if (state == State.goldFromStorage) player.storageGuild.CmdTransferGoldFromBankToInv(uint.Parse(inputFieldGold.text));
            else player.storageGuild.CmdTransferGoldFromInvToBank(uint.Parse(inputFieldGold.text));

            inputFieldGold.text = "0";
            inputFieldGold.gameObject.SetActive(false);

            buttonConfirmChangeGold.gameObject.SetActive(false);
            buttonPlus.gameObject.SetActive(true);
            buttonMinus.gameObject.SetActive(true);
        }
    }
}