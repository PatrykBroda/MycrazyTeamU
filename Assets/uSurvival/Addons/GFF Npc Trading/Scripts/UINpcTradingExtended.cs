using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UINpcTradingExtended : MonoBehaviour
    {
        public int amountSlotForSelling = 25;
        public KeyCode hotKeyConfirmAmount = KeyCode.KeypadEnter;

        [Header("use Fast Buy ?")]
        public bool useFastBuy = true;
        public KeyCode fastInteractHotKey = KeyCode.LeftControl;

        [Header("Settings : addons")]
        public bool useToolTipsExtended;

        [Header("UI Elements")]
        public UINpcTradingExtendedSlot slotPrefab;
        public Transform content;
        public Button buttonSell;
        public Button buttonBuy;

        [Header("Panel Total")]
        public TextMeshProUGUI totalGoldValue;
        public Text totalCoinsValue;
        public Button totalButtonCanсel;
        public Button totalButtonOK;

        [Header("Panel Amount")]
        public GameObject panelAmount;
        public InputField amountInputField;
        public Button amountOKButton;
        public Button amountCancelButton;

        [Header("Panel Info")]
        public GameObject panelInfo;
        public Text textInfo;
        public Button infoOKButton;
        public Button infoCancelButton;

        [Header("Panel Error")]
        public GameObject panelError;
        public Text textError;

        //singleton
        public static UINpcTradingExtended singleton;
        public UINpcTradingExtended() { singleton = this; }

        private void OnEnable()
        {
            Player.localPlayer.CmdClearBuyIndices();
            Player.localPlayer.tradingState = Player.State.buy;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                // use collider point(s) to also work with big entities
                if (player.interactionExtended.target != null && player.interactionExtended.target is Npc npc && Utils.ClosestDistance(player.collider, npc.collider) <= player.interactionExtended.range && player.health.current > 0)
                {
                    buttonBuy.interactable = (player.tradingState == Player.State.sell);
                    buttonBuy.onClick.SetListener(() =>
                    {
                        //if we clicked to buy but there are not purchased goods
                        if (player.CostOfSellingItems() > 0)
                        {
                            ShowInfoMessage(Localization.Translate("NotSoldItems"));
                        }
                        else
                        {
                            player.tradingState = Player.State.buy;
                            player.sellSlots = new List<SellSlot>();
                        }
                    });

                    buttonSell.interactable = (player.tradingState == Player.State.buy);
                    buttonSell.onClick.SetListener(() =>
                    {
                        //if we clicked sell but there are items on buy
                        if (player.CostOfPurchasedItems() > 0)
                        {
                            ShowInfoMessage(Localization.Translate("NotBuyItems"));
                        }
                        else
                        {
                            player.tradingState = Player.State.sell;
                            player.CmdClearBuyIndices();
                        }
                    });

                    if (player.tradingState == Player.State.buy)
                    {
                        // items for sale
                        UIUtils.BalancePrefabs(slotPrefab.gameObject, npc.trading.saleItems.Length, content);

                        // refresh all items
                        for (int i = 0; i < npc.trading.saleItems.Length; ++i)
                        {
                            if (npc.trading.saleItems[i] != null)
                            {
                                UINpcTradingExtendedSlot slot = content.GetChild(i).GetComponent<UINpcTradingExtendedSlot>();
                                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                                slot.dragAndDropable.gameObject.tag = "BuySlot";

                                ItemSlot itemslot = new ItemSlot();
                                itemslot.item = new Item(npc.trading.saleItems[i]);
                                itemslot.amount = npc.trading.saleItems[i].maxStack;

                                slot.dragAndDropable.dragable = true;
                                slot.image.color = Color.white;
                                slot.image.sprite = itemslot.item.image;

                                //ToolTip
                                // only build tooltip while it's actually shown. this avoids MASSIVE amounts of StringBuilder allocations.
                                //use GFF ToolTips ?
                                if (!useToolTipsExtended)
                                {
                                    slot.tooltip.enabled = true;
                                    if (slot.tooltip.IsVisible())
                                        slot.tooltip.text = itemslot.ToolTip();
                                }
                                else
                                {
                                    slot.tooltipExtended.enabled = true;
                                    if (slot.tooltipExtended.IsVisible())
                                        slot.tooltipExtended.slot = itemslot;
                                }

                                int icopy = i;
                                slot.button.interactable = true;
                                slot.button.onClick.SetListener(() =>
                                {
                                    ItemSlot temp = new ItemSlot();
                                    temp.item = new Item(npc.trading.saleItems[icopy]);

                                    //if the quick buy key is pressed
                                    if (useFastBuy && Input.GetKey(fastInteractHotKey) && !UIUtils.AnyInputActive())
                                    {
                                        player.buyState = NpcTradingExtendedBuyState.FastBuyAndClicked;

                                        //int freeSlot = player.FreeSlotForBuy();
                                        int freeSlot = player.FreeSlotForBuyWithIncreaseInventory();
                                        if (freeSlot != -1)
                                        {
                                            //find the amount we can buy
                                            ushort amount = player.FindAmountCanBuy(npc.trading.saleItems[icopy]);
                                            if (amount > 0)
                                            {
                                                temp.amount = amount;
                                                player.CmdUpdateBuyingSlot(freeSlot, temp);
                                            }
                                            else ErrorNotEnoughGold();
                                        }
                                        else ErrorInventoryFull();
                                    }
                                    else
                                    {
                                        player.buyState = NpcTradingExtendedBuyState.Clicked;

                                        //int freeSlot = player.FreeSlotForBuy();
                                        int freeSlot = player.FreeSlotForBuyWithIncreaseInventory();
                                        if (freeSlot != -1)
                                        {
                                            //if (npc.saleItems[icopy] is AmmoItem == false)
                                            {
                                                //find the amount we can buy
                                                if (player.FindAmountCanBuy(npc.trading.saleItems[icopy]) > 0)
                                                {
                                                    player.tempBuyItem = new Item(npc.trading.saleItems[icopy]);

                                                    if (player.tempBuyItem.maxStack == 1 || player.tempBuyItem.data is EquipmentItem)
                                                    {
                                                        temp.amount = 1;
                                                        player.CmdUpdateBuyingSlot(freeSlot, temp);
                                                    }
                                                    else
                                                    {
                                                        panelAmount.SetActive(true);
                                                        amountInputField.text = player.tempBuyItem.maxStack.ToString();
                                                        amountInputField.ActivateInputField();
                                                    }
                                                }
                                                else ErrorNotEnoughGold();
                                            }
                                            //else
                                            //{
                                            //    //find the amount we can buy
                                            //    if (player.gold >= npc.saleItems[icopy].maxStack * npc.saleItems[icopy].buyPrice)
                                            //    {
                                            //        player.tempBuyItem = new Item(npc.saleItems[icopy]);

                                            //        temp.amount = npc.saleItems[icopy].maxStack;
                                            //        player.CmdUpdateBuyingSlot(freeSlot, temp);
                                            //    }
                                            //    else ErrorNotEnoughGold();
                                            //}
                                        }
                                        else ErrorInventoryFull();
                                    }
                                });

                                // addon system hooks (Item rarity ana Upgrade)
                                UtilsExtended.InvokeMany(typeof(UINpcTradingExtended), this, "UpdateItemSlot_", player, slot, itemslot);
                            }
                        }

                        //panel total
                        totalGoldValue.text = player.CostOfPurchasedItems().ToString();
                    }
                    else if (player.tradingState == Player.State.sell)
                    {
                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(slotPrefab.gameObject, amountSlotForSelling, content);

                        // refresh all items
                        for (int i = 0; i < amountSlotForSelling; ++i)
                        {
                            UINpcTradingExtendedSlot slot = content.GetChild(i).GetComponent<UINpcTradingExtendedSlot>();
                            slot.dragAndDropable.name = i.ToString(); // drag and drop index
                            slot.dragAndDropable.tag = "SellSlot";
                            ItemSlot TempSlot = new ItemSlot();

                            if (player.sellSlots.Count > 0 && player.sellSlots.Count > i && player.sellSlots[i].index >= 0 && player.sellSlots[i].index < player.inventory.slots.Count && player.inventory.slots[player.sellSlots[i].index].amount > 0)
                            {
                                TempSlot = player.inventory.slots[player.sellSlots[i].index];

                                slot.button.onClick.SetListener(() =>
                                {
                                    //clear slot
                                    player.sellSlots.RemoveAt(int.Parse(slot.dragAndDropable.name));
                                });

                                // refresh valid item
                                //use GFF ToolTips ?
                                if (!useToolTipsExtended)
                                {
                                    slot.tooltip.enabled = true;
                                    if (slot.tooltip.IsVisible())
                                        slot.tooltip.text = TempSlot.ToolTip();
                                }
                                else
                                {
                                    slot.tooltipExtended.enabled = true;
                                    if (slot.tooltipExtended.IsVisible())
                                        slot.tooltipExtended.slot = TempSlot;
                                }

                                slot.dragAndDropable.dragable = true;
                                slot.image.color = Color.white;
                                slot.image.sprite = TempSlot.item.image;
                                slot.amountOverlay.SetActive(player.sellSlots[i].amount > 1);
                                slot.amountText.text = player.sellSlots[i].amount.ToString();
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
                                slot.upgradeText.text = "";
                            }

                            // addon system hooks (Item rarity ana Upgrade)
                            UtilsExtended.InvokeMany(typeof(UINpcTradingExtended), this, "UpdateItemSlot_", player, slot, TempSlot);
                        }

                        //panel total
                        totalGoldValue.text = player.CostOfSellingItems().ToString();
                    }

                    if (panelInfo.activeSelf)
                    {
                        infoCancelButton.onClick.SetListener(() =>
                        {
                            panelInfo.SetActive(false);
                            player.CmdClearBuyIndices();
                            player.sellSlots = new List<SellSlot>();

                            if (player.tradingState == Player.State.sell) player.tradingState = Player.State.buy;
                            else player.tradingState = Player.State.sell;
                        });

                        infoOKButton.onClick.SetListener(() =>
                        {
                            if (player.tradingState == Player.State.buy)
                            {
                                player.CmdBuyComplete();
                                player.tradingState = Player.State.sell;
                            }
                            else
                            {
                                SellComplete(player);
                                player.tradingState = Player.State.buy;
                            }

                            panelInfo.SetActive(false);
                        });
                    }

                    if (panelAmount.activeSelf)
                    {
                        if (player.tradingState == Player.State.buy)
                        {
                            // ChangeInputAmount
                            if (player.tempBuyItem.buyPrice * amountInputField.text.ToInt() >= (player.gold - player.CostOfPurchasedItems()))
                                amountInputField.text = ((player.gold - player.CostOfPurchasedItems()) / player.tempBuyItem.buyPrice).ToString();

                            // hotkey (not while typing in chat, etc.)
                            if (Input.GetKeyDown(hotKeyConfirmAmount) && !UIUtils.AnyInputActive()) BuyAmountAplly(player);
                            amountOKButton.onClick.SetListener(() => { BuyAmountAplly(player); });

                            amountCancelButton.onClick.SetListener(() => { panelAmount.SetActive(false); });

                            if (amountInputField.text.ToInt() > player.tempBuyItem.maxStack) amountInputField.text = player.tempBuyItem.maxStack.ToString();
                        }
                        else if (player.tradingState == Player.State.sell)
                        {
                            // ChangeInputAmount if bigger then max
                            if (amountInputField.text.ToInt() > player.inventory.slots[player.sellIndexFrom].amount)
                                amountInputField.text = player.inventory.slots[player.sellIndexFrom].amount.ToString();

                            int index = player.sellSlots.FindIndex(x => x.index == player.sellIndexFrom);

                            // hotkey (not while typing in chat, etc.)
                            if (Input.GetKeyDown(hotKeyConfirmAmount) && !UIUtils.AnyInputActive())
                            {
                                panelAmount.SetActive(false);
                                player.sellSlots[index].amount = amountInputField.text.ToUshort();
                            }

                            amountOKButton.onClick.SetListener(() =>
                            {
                                panelAmount.SetActive(false);
                                player.sellSlots[index].amount = amountInputField.text.ToUshort();
                            });

                            amountCancelButton.onClick.SetListener(() =>
                            {
                                panelAmount.SetActive(false);
                                player.sellSlots.RemoveAt(index);
                            });
                        }
                    }

                    totalButtonCanсel.onClick.SetListener(() =>
                    {
                        if (player.tradingState == Player.State.buy) player.CmdClearBuyIndices();
                        else player.sellSlots = new List<SellSlot>();
                    });
                    totalButtonOK.onClick.SetListener(() =>
                    {
                        if (player.tradingState == Player.State.sell && player.CostOfSellingItems() > 0)
                        {
                            SellComplete(player);
                        }
                        if (player.tradingState == Player.State.buy && player.CostOfPurchasedItems() > 0)
                        {
                            if (player.gold >= player.CostOfPurchasedItems()) player.CmdBuyComplete();
                            else ErrorNotEnoughGold();
                        }
                    });
                }
                else
                {
                    gameObject.SetActive(false);
                    player.tradingState = Player.State.close;
                }
            }
            else gameObject.SetActive(false);
        }

        private void BuyAmountAplly(Player player)
        {
            ItemSlot temp = new ItemSlot();
            temp.item = player.tempBuyItem;
            temp.amount = amountInputField.text.ToUshort();

            if (player.buyState == NpcTradingExtendedBuyState.DragAndDrop)
            {
                player.CmdUpdateBuyingSlot(player.buyIndex, temp);
            }
            else
            {
                //int freeSlot = player.FreeSlotForBuy();
                int freeSlot = player.FreeSlotForBuyWithIncreaseInventory();
                if (freeSlot != -1) player.CmdUpdateBuyingSlot(freeSlot, temp);
                else ErrorInventoryFull();
            }

            panelAmount.SetActive(false);
        }

        private void SellComplete(Player player)
        {
            for (int i = 0; i < player.sellSlots.Count; ++i)
            {
                ushort amount = player.sellSlots[i].amount;

                //if this is ammo
                if (player.inventory.slots[player.sellSlots[i].index].item.data is AmmoItem)
                    amount = player.inventory.slots[player.sellSlots[i].index].amount;

                player.CmdNpcSellItem(player.sellSlots[i].index, amount);
            }

            player.sellSlots = new List<SellSlot>();
        }

        public void CheckBeforeClose()
        {
            Player player = Player.localPlayer;

            if (player.CostOfPurchasedItems() > 0)
            {
                ShowInfoMessage(Localization.Translate("You have not buy items") + " \n +" + Localization.Translate("Want to confirm the buy") + " ?");
            }
            else if (player.CostOfSellingItems() > 0)
            {
                ShowInfoMessage("You have not sold items \n Want to confirm the sale ?");
            }
            else Close(player);
        }
        private void Close(Player player)
        {
            player.sellSlots = new List<SellSlot>();

            //remove all prefabs in panel sell
            for (int i = 0; i < content.childCount; ++i) Destroy(content.GetChild(i).gameObject);

            gameObject.SetActive(false);

            player.tradingState = Player.State.close;
        }

        private void ShowInfoMessage(string message)
        {
            textInfo.text = message;
            panelInfo.SetActive(true);
        }

        //errors
        private void ShowErrorMessage(string message)
        {
            textError.text = message;
            panelError.SetActive(true);
        }
        public void ErrorNotEnoughGold()
        {
            ShowErrorMessage(Localization.Translate("NotEnoughGold"));
        }
        public void ErrorInventoryFull()
        {
            ShowErrorMessage(Localization.Translate("InventoryFull"));
        }
        public void ErrorSlotAlreadyTaken()
        {
            ShowErrorMessage(Localization.Translate("This slot is already taken"));
        }
    }
}

