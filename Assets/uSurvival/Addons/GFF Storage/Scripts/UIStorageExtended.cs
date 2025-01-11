using Mirror;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public enum SortBy { amount, rarity, type }

    public partial class UIStorageExtended : MonoBehaviour
    {
        [SerializeField] private KeyCode hotKeyConfirmAmount = KeyCode.KeypadEnter;
        [SerializeField] private float timeForInfoText = 2.0f;

        [Header("Start Panel")]
        [SerializeField] private GameObject panelStart;
        [SerializeField] private Button buttonStorageBuy;

        [Header("Panel Storage Buy")]
        [SerializeField] private GameObject panelStorageBuy;
        [SerializeField] private TextMeshProUGUI textGoldPrice;
        [SerializeField] private Button buttonBuySlotsConfirm;
        [SerializeField] private TextMeshProUGUI textInfo;

        [Header("Panel Storage")]
        [SerializeField] private GameObject panelStorage;
        [SerializeField] private UniversalSlot slotPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private TextMeshProUGUI textStorageCount;
        [SerializeField] private TextMeshProUGUI textGoldInStorage;
        [SerializeField] private TMP_InputField inputFieldGold;
        [SerializeField] private Button buttonPlus;
        [SerializeField] private Button buttonMinus;
        [SerializeField] private Button buttonConfirmChangeGold;
        [SerializeField] private Button buttonStorageExpand;

        [Header("Components")]
        [SerializeField] private AudioSource audioSource;

        private double timeInfo;

        private enum State { goldToStorage, goldFromStorage }
        private State state;

        //singleton
        public static UIStorageExtended singleton;
        public UIStorageExtended()
        {
            singleton = this;
        }

        public void Show(Player player)
        {
            panelStorageBuy.SetActive(false);

            //set the ability to deposit and withdraw gold
            if (player.storage.GetMaxCapacityForGold() == 0)
            {
                buttonPlus.gameObject.SetActive(false);
                buttonMinus.gameObject.SetActive(false);
            }

            //if the storage size is 0 (player has not bought storage yet)
            if (player.storage.storageSize == 0)
            {
                panelStorage.SetActive(false);
                panelStart.SetActive(true);
                buttonStorageBuy.interactable = true;
            }
            else
            {
                panelStart.SetActive(false);
                panelStorage.SetActive(true);
            }
        }

        private void Start()
        {
            buttonStorageBuy.onClick.SetListener(() =>
            {
                audioSource.Play();
                panelStorage.SetActive(true);
                panelStorageBuy.SetActive(true);
                panelStart.SetActive(false);
            });

            buttonStorageExpand.onClick.SetListener(() =>
            {
                audioSource.Play();
                panelStorageBuy.SetActive(true);
            });

            //Take gold from storage
            buttonMinus.onClick.SetListener(() =>
            {
                audioSource.Play();
                state = State.goldFromStorage;
                buttonConfirmChangeGold.gameObject.SetActive(true);
                buttonPlus.gameObject.SetActive(false);
                buttonMinus.gameObject.SetActive(false);

                inputFieldGold.text = "0";
                inputFieldGold.gameObject.SetActive(true);
                inputFieldGold.ActivateInputField();
            });
        }

        private void Update()
        {
            if (panelStorage.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null && player.health.current > 0 &&
                    player.interactionExtended.target != null && player.interactionExtended.target is Npc npc && npc.storage &&
                    uSurvival.Utils.ClosestDistance(player.collider, npc.collider) <= player.interactionRange)
                {
                    if (panelStorageBuy.activeSelf)
                    {
                        uint goldRequiredToBuy = player.storage.GetStoragePurchaseCost();
                        textGoldPrice.text = goldRequiredToBuy == 0 ? "0" : goldRequiredToBuy.ToString("###,###,###,###");

                        buttonBuySlotsConfirm.interactable = player.storage.CanBuySlots();
                        buttonBuySlotsConfirm.onClick.SetListener(() =>
                        {
                            audioSource.Play();
                            buttonBuySlotsConfirm.interactable = false;
                            textInfo.gameObject.SetActive(true);
                            player.storage.CmdStorageBuy();
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

                    textStorageCount.text = player.storage.storageSize + "/" + player.storage.GetMaxSlotsAmountForStorage();

                    //instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(slotPrefab.gameObject, player.storage.storageSize, content);

                    //refresh all items
                    for (int i = 0; i < player.storage.slots.Count; ++i)
                    {
                        UniversalSlot slot = content.GetChild(i).GetComponent<UniversalSlot>();
                        slot.dragAndDropable.name = i.ToString(); // drag and drop index
                        slot.dragAndDropable.tag = "StorageSlot";
                        ItemSlot storageSlot = player.storage.slots[i];

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
                        UtilsExtended.InvokeMany(typeof(UIStorageExtended), this, "Update_", player, slot, storageSlot);
                    }

                    //gold
                    textGoldInStorage.text = player.storage.goldInStorage == 0 ? "0" : player.storage.goldInStorage.ToString("###,###,###,###");

                    //Put gold to storage
                    buttonPlus.interactable = player.storage.CanPutGoldToStorage();
                    buttonPlus.onClick.SetListener(() =>
                    {
                        audioSource.Play();
                        state = State.goldToStorage;
                        buttonConfirmChangeGold.gameObject.SetActive(true);
                        buttonPlus.gameObject.SetActive(false);
                        buttonMinus.gameObject.SetActive(false);

                        inputFieldGold.text = "0";
                        inputFieldGold.gameObject.SetActive(true);
                        inputFieldGold.ActivateInputField();
                    });

                    //verification of the entered data when collecting or depositing gold
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
                            if (goldInInputField > player.gold)
                            {
                                inputFieldGold.text = player.gold.ToString();
                                goldInInputField = player.gold;
                            }

                            //if it is indicated more than it is possible to place
                            if (goldInInputField + player.storage.goldInStorage > player.storage.GetMaxCapacityForGold())
                            {
                                inputFieldGold.text = (player.storage.GetMaxCapacityForGold() - player.storage.goldInStorage).ToString();
                            }
                        }
                        //if take away from storage
                        else
                        {
                            //if indicated more than available in storage
                            if (goldInInputField > player.storage.goldInStorage) inputFieldGold.text = player.storage.goldInStorage.ToString();
                        }
                    }

                    //if the player has confirmed the transfer of gold
                    if (Input.GetKeyDown(hotKeyConfirmAmount) && !UIUtils.AnyInputActive()) GoldExchangeConfirmation(player);
                    buttonConfirmChangeGold.onClick.SetListener(() => { GoldExchangeConfirmation(player); });

                    //add slots to storage
                    buttonStorageExpand.interactable = player.storage.CanBuyMoreSlots();
                }
                else panelStorage.SetActive(false);
            }
        }

        private void GoldExchangeConfirmation(Player player)
        {
            if (state == State.goldFromStorage) player.storage.CmdTransferGoldFromStorage(uint.Parse(inputFieldGold.text));
            else player.storage.CmdTransferGoldToStorage(uint.Parse(inputFieldGold.text));

            inputFieldGold.text = "0";
            inputFieldGold.gameObject.SetActive(false);

            buttonConfirmChangeGold.gameObject.SetActive(false);
            buttonPlus.gameObject.SetActive(true);
            buttonMinus.gameObject.SetActive(true);
        }

        public void Sort()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                player.storage.CmdStorageSort();
            }
        }
    }
}