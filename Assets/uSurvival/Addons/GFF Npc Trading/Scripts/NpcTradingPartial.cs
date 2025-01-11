using GFFAddons;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using uSurvival;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        public uint buyPrice;
        public uint sellPrice;
        public bool sellable = true;
    }

    public partial struct Item
    {
        public uint buyPrice => data.buyPrice;
        public uint sellPrice => data.sellPrice;
        public bool sellable => data.sellable;
    }

    public partial class Player
    {
        public readonly SyncList<ItemSlot> npcBuying = new SyncList<ItemSlot>();
        [HideInInspector] public List<SellSlot> sellSlots = new List<SellSlot>();

        public enum State : byte { close, buy, sell }
        [HideInInspector] public State tradingState;

        [HideInInspector] public NpcTradingExtendedBuyState buyState;
        [HideInInspector] public int sellIndexFrom = -1;
        [HideInInspector] public int buyIndex = -1;
        [HideInInspector] public Item tempBuyItem;

        void OnDragAndDrop_InventorySlot_SellSlot(int[] slotIndices)
        {
            //slotIndices[0] - from slot
            //slotIndices[1] - to slot

            if (!sellSlots.Exists(x => x.index == slotIndices[0]))
            {
                SellSlot temp = new SellSlot();
                temp.index = slotIndices[0];

                if (inventory.slots[slotIndices[0]].amount == 1 || inventory.slots[slotIndices[0]].item.data is AmmoItem ammo)
                {
                    temp.amount = inventory.slots[slotIndices[0]].amount;
                    sellSlots.Add(temp);
                }
                else
                {
                    sellSlots.Add(temp);
                    sellIndexFrom = slotIndices[0];
                    UINpcTradingExtended.singleton.amountInputField.text = inventory.slots[slotIndices[0]].amount.ToString();
                    UINpcTradingExtended.singleton.panelAmount.SetActive(true);
                    UINpcTradingExtended.singleton.amountInputField.ActivateInputField();
                }
            }
        }
        void OnDragAndDrop_BuySlot_InventorySlot(int[] slotIndices)
        {
            //check if the slot is free
            if (inventory.slots[slotIndices[1]].amount < 1 && npcBuying[slotIndices[1]].amount < 1)
            {
                Item item = new Item(((Npc)interactionExtended.target).trading.saleItems[slotIndices[0]]);
                tempBuyItem = item;
                buyState = NpcTradingExtendedBuyState.DragAndDrop;

                //check if there is enough gold to buy 1 item
                if (gold > item.buyPrice + CostOfPurchasedItems())
                {
                    if (item.maxStack > 1 && item.data is AmmoItem == false)
                    {
                        UINpcTradingExtended.singleton.panelAmount.SetActive(true);
                        UINpcTradingExtended.singleton.amountInputField.text = FindAmountCanBuy(item.data).ToString();
                        UINpcTradingExtended.singleton.amountInputField.ActivateInputField();
                        buyIndex = slotIndices[1];
                    }
                    else
                    {
                        ItemSlot itemslot = new ItemSlot();
                        itemslot.item = item;

                        if (item.data is AmmoItem == false) itemslot.amount = 1;
                        else itemslot.amount = item.maxStack;

                        //item Enchantmen addon
                        //itemslot.item.holes = npc.amountofSlotsForEnchantmentInItemsSold;

                        CmdUpdateBuyingSlot(slotIndices[1], itemslot);
                    }
                }
                else UINpcTradingExtended.singleton.ErrorNotEnoughGold();
            }
            else UINpcTradingExtended.singleton.ErrorSlotAlreadyTaken();
        }

        [Command]
        public void CmdUpdateBuyingSlot(int index, ItemSlot temp)
        {
            npcBuying[index] = temp;
        }

        [Command]
        public void CmdBuyComplete()
        {
            for (int i = 0; i < npcBuying.Count; ++i)
            {
                if (npcBuying[i].amount > 0)
                {
                    uint requiredGold = (npcBuying[i].item.buyPrice * npcBuying[i].amount);
                    if (gold >= requiredGold)
                    {
                        gold -= requiredGold;
                        if (i <= inventory.slots.Count && inventory.slots[i].amount == 0) inventory.slots[i] = npcBuying[i];
                    }
                }
            }

            ClearBuyIndices();
        }

        [Command]
        public void CmdClearBuyIndices()
        {
            ClearBuyIndices();
        }

        private void ClearBuyIndices()
        {
            npcBuying.Clear();
            for (int i = 0; i < inventory.slots.Count; ++i)
                npcBuying.Add(new ItemSlot());
        }

        public long CostOfPurchasedItems()
        {
            long value = 0;
            for (int i = 0; i < npcBuying.Count; i++)
            {
                if (npcBuying[i].amount > 0)
                {
                    //for ammo addon
                    //if (npcBuying[i].item.data is AmmoItem)
                    //    value += npcBuying[i].amount * npcBuying[i].item.buyPrice;
                    //else 
                    value += npcBuying[i].item.buyPrice * npcBuying[i].amount;
                }
            }

            return value;
        }
        public long CostOfSellingItems()
        {
            long value = 0;
            for (int i = 0; i < sellSlots.Count; i++)
            {
                if (sellSlots[i].amount > 0)
                {
                    value += sellSlots[i].amount * inventory.slots[sellSlots[i].index].item.sellPrice;
                }
            }

            return value;
        }

        public int FreeSlotForBuy()
        {
            for (int i = 0; i < inventory.slots.Count; i++)
            {
                if (inventory.slots[i].amount == 0 && npcBuying[i].amount == 0) return i;
            }

            return -1;
        }

        public int FreeSlotForBuyWithIncreaseInventory()
        {
            for (short i = 0; i < inventory.sizeDefault; ++i)
            {
                if (inventory.slots[i].amount == 0 && npcBuying[i].amount == 0) return i;
            }

            int countSlots = 0;
            for (short i = 0; i < equipment.slots.Count; ++i)
            {
                if (equipment.slots[i].amount > 0 && equipment.slots[i].item.data is EquipmentItem eItem && eItem.addSlots > 0)
                {
                    ushort goodSlotsAmount = eItem.GetGoodDurability(equipment.slots[i].item.durability);
                    for (short x = 0; x < goodSlotsAmount; ++x)
                    {
                        int slotIndex = inventory.sizeDefault + countSlots + x;

                        if (inventory.slots[slotIndex].amount == 0 && npcBuying[slotIndex].amount == 0) return slotIndex;
                    }
                    countSlots += eItem.addSlots;
                }
            }

            return -1;
        }

        public ushort FindAmountCanBuy(ScriptableItem item)
        {
            if (item.maxStack > 1)
            {
                if (gold - CostOfPurchasedItems() >= item.buyPrice * item.maxStack) return item.maxStack;
                else return (ushort)(gold - CostOfPurchasedItems() / item.buyPrice);
            }
            else
            {
                if (gold - CostOfPurchasedItems() >= item.buyPrice) return 1;
            }

            return 0;
        }

        [Command]
        public void CmdNpcSellItem(int index, ushort amount)
        {
            // validate: close enough, npc alive and valid index and valid item?
            // use collider point(s) to also work with big entities
            if (health.current > 0 &&
                interactionExtended.target != null && interactionExtended.target is Npc npc &&
                Utils.ClosestDistance(this.collider, npc.collider) <= interactionExtended.range &&
                0 <= index && index < inventory.slots.Count)
            {
                // sellable?
                ItemSlot slot = inventory.slots[index];
                if (slot.amount > 0)
                {
                    // valid amount?
                    if (1 <= amount && amount <= slot.amount)
                    {
                        // sell the amount
                        gold += (slot.item.sellPrice * amount);
                        slot.DecreaseAmount(amount);
                        inventory.slots[index] = slot;
                    }
                }
            }
        }
    }
}

namespace GFFAddons
{
    public class SellSlot
    {
        public int index = -1;
        public ushort amount = 1;
    }

    public enum NpcTradingExtendedBuyState : byte { DragAndDrop, FastBuyAndClicked, Clicked }

    public partial class Npc
    {
        public NpcTrading trading;
    }

    public partial class UIInventoryExtended
    {
        void IsUsedSlot_NpcTrading(Player player, int inventoryIndex)
        {
            if (isUsedSlot == false)
            {
                if (UINpcTradingExtended.singleton.gameObject.activeSelf)
                {
                    for (int i = 0; i < player.sellSlots.Count; i++)
                        if (inventoryIndex == player.sellSlots[i].index)
                        {
                            isUsedSlot = true;
                            break;
                        }
                }
            }
        }
    }
}