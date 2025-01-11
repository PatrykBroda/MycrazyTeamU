using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public partial class UIInventoryExtended : MonoBehaviour
    {
        [SerializeField] private UniversalSlot slotPrefab;
        [SerializeField] private Transform content;

        [Header("Settings : Npc trading")]
        [SerializeField] private KeyCode fastInteractHotKey = KeyCode.LeftControl;
        [SerializeField] private bool useTradingSystemColors = true;
        [SerializeField] private Color colorSell = Color.red;
        [SerializeField] private Color colorPurchasedItem = Color.yellow;
        [SerializeField] private Color colorNormalItem = Color.gray;

        [SerializeField] private GameObject itemsStoragePrefab;
        [SerializeField] private Transform contentStoraes;

        [SerializeField] private TextMeshProUGUI textWeightValue;

        private bool isUsedSlot = false;

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                //item weight addon
                textWeightValue.text = player.weight.CurrentWeightToString() + "/" + player.weight.CurrentWeightMaxToString();
                if (player.weight.weightMax < player.weight.weightCurrent) textWeightValue.color = Color.red;
                else textWeightValue.color = Color.white;

                int amountStorages = player.equipment.AmountStorages();

                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(itemsStoragePrefab.gameObject, amountStorages, contentStoraes);

                // instantiate/destroy enough slots for default slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.sizeDefault, content);

                // refresh all items
                for (short i = 0; i < player.inventory.sizeDefault; ++i)
                {
                    UniversalSlot slot = content.GetChild(i).GetComponent<UniversalSlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index
                    ItemSlot itemSlot = player.inventory.slots[i];

                    short icopy = i; // needed for lambdas, otherwise i is Count

                    if (itemSlot.amount > 0)
                    {
                        slot.imageBinding.SetActive(itemSlot.item.binding);

                        // refresh valid item
                        //slot.tooltip.enabled = false;
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        //if (slot.tooltip.IsVisible())
                        //slot.tooltip.text = itemSlot.ToolTip();

                        slot.tooltipExtended.enabled = true;
                        if (slot.tooltipExtended.IsVisible())
                            slot.tooltipExtended.slot = itemSlot;

                        //slot.sliderDurability.gameObject.SetActive(itemSlot.item.durability > 1);
                        slot.textDurability.text = itemSlot.item.durability > 1 ? ((int)(itemSlot.item.DurabilityPercent() * 100)).ToString() + "%" : "";
                        slot.image.sprite = itemSlot.item.image;
                        slot.image.color = Color.white; // reset for non-durability items

                        if (itemSlot.item.data is AmmoItem ammo && ammo.damageBonusFromAmmo > 0) slot.textAmmoType.text = "+" + ammo.damageBonusFromAmmo * 100 + "%";
                        else slot.textAmmoType.text = "";

                        // cooldown if usable item
                        if (itemSlot.item.data is UsableItem usable2)
                        {
                            float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                            slot.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                        }
                        else slot.cooldownCircle.fillAmount = 0;

                        //if item is not used in another panel
                        isUsedSlot = false;

                        // addon system hooks (isUsedSlot)
                        UtilsExtended.InvokeMany(typeof(UIInventoryExtended), this, "IsUsedSlot_", player, i);

                        if (isUsedSlot == false)
                        {
                            slot.dragAndDropable.dragable = true;
                            slot.dragAndDropable.dropable = true;
                            slot.button.interactable = true;
                            slot.amountOverlay.SetActive(itemSlot.amount > 1);
                            slot.amountText.text = itemSlot.amount.ToString();

                            //paint color
                            slot.GetComponent<Image>().color = colorNormalItem;
                        }
                        else
                        {
                            slot.dragAndDropable.dragable = false;
                            slot.dragAndDropable.dropable = false;
                            slot.button.interactable = false;
                            slot.amountOverlay.SetActive(false);

                            //paint color
                            slot.GetComponent<Image>().color = colorSell;
                        }

                        slot.button.onClick.SetListener(() =>
                        {
                            if (player.tradingState == Player.State.sell && Input.GetKey(fastInteractHotKey))
                            {
                                SellSlot temp = new SellSlot();
                                temp.index = icopy;
                                temp.amount = itemSlot.amount;
                                player.sellSlots.Add(temp);
                            }
                            else
                            {
                                if (itemSlot.item.data is ScriptableDurabilityRestorer restorer)
                                {
                                    UIItemDurabilityRestoration.singleton.panel.SetActive(true);
                                    player.inventory.inventoryIndexDurabilityRestoration = icopy;
                                }
                                else if (itemSlot.item.data is UsableItem usable && usable.CanUseInventory(player, icopy) == Usability.Usable)
                                {
                                    player.inventory.CmdUseItem(icopy);
                                }
                            }
                        });
                    }
                    else
                    {
                        slot.sliderDurability.gameObject.SetActive(false);
                        slot.imageBinding.SetActive(false);

                        // refresh items from npc trading buy
                        if (player.tradingState == Player.State.buy && i < player.npcBuying.Count && player.npcBuying[i].amount > 0)
                        {
                            // only build tooltip while it's actually shown. this
                            // avoids MASSIVE amounts of StringBuilder allocations.
                            //slot.tooltip.enabled = true;
                            //if (slot.tooltip.IsVisible())
                            //    slot.tooltip.text = player.npcBuying[i].ToolTip();

                            slot.tooltipExtended.enabled = true;
                            if (slot.tooltipExtended.IsVisible())
                                slot.tooltipExtended.slot = player.npcBuying[i];

                            slot.dragAndDropable.dragable = false;
                            slot.dragAndDropable.dropable = false;

                            slot.image.color = Color.white;
                            slot.image.sprite = player.npcBuying[i].item.image;

                            slot.amountOverlay.SetActive(player.npcBuying[i].amount > 1);
                            slot.amountText.text = player.npcBuying[i].amount.ToString();

                            slot.button.interactable = true;
                            slot.button.onClick.SetListener(() =>
                            {
                                //remove from inventory buying item
                                player.CmdUpdateBuyingSlot(icopy, new ItemSlot());
                            });

                            //paint color
                            if (useTradingSystemColors) slot.GetComponent<Image>().color = colorPurchasedItem;
                        }
                        else
                        {
                            // refresh invalid item
                            slot.button.onClick.RemoveAllListeners();
                            slot.tooltip.enabled = false;
                            slot.tooltipExtended.enabled = false;
                            slot.dragAndDropable.dragable = false;
                            slot.dragAndDropable.dropable = true;
                            slot.image.color = Color.clear;
                            slot.image.sprite = null;
                            slot.cooldownCircle.fillAmount = 0;
                            slot.amountOverlay.SetActive(false);
                            slot.textDurability.text = "";
                            slot.textAmmoType.text = "";

                            //paint color
                            slot.GetComponent<Image>().color = colorNormalItem;
                        }
                    }
                }

                int countSlots = 0;
                int amountStorageIndex = 1;
                for (short i = 0; i < player.equipment.slots.Count; ++i)
                {
                    if (player.equipment.slots[i].amount > 0 && player.equipment.slots[i].item.data is EquipmentItem eItem && eItem.addSlots > 0)
                    {
                        UIInventoryItemDataSlot storage = contentStoraes.GetChild(amountStorageIndex).GetComponent<UIInventoryItemDataSlot>();
                        storage.textItemname.text = Localization.Translate(eItem.category);

                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(slotPrefab.gameObject, eItem.addSlots, storage.itemsgrid);

                        ushort goodSlotsAmount = eItem.GetGoodDurability(player.equipment.slots[i].item.durability);

                        short icopy = i; // needed for lambdas, otherwise i is Count

                        for (short x = 0; x < eItem.addSlots; ++x)
                        {
                            int slotIndex = player.inventory.sizeDefault + countSlots + x;
                            UniversalSlot slot = storage.itemsgrid.GetChild(x).GetComponent<UniversalSlot>();
                            slot.dragAndDropable.name = slotIndex.ToString(); // drag and drop index
                            ItemSlot itemSlot = player.inventory.slots[slotIndex];

                            short xcopy = x; // needed for lambdas, otherwise i is Co

                            if (itemSlot.amount > 0)
                            {
                                isUsedSlot = false;

                                slot.imageBinding.SetActive(itemSlot.item.binding);

                                // addon system hooks (isUsedSlot)
                                UtilsExtended.InvokeMany(typeof(UIInventoryExtended), this, "IsUsedSlot_", player, i);

                                // refresh valid item
                                slot.tooltip.enabled = false;
                                // only build tooltip while it's actually shown. this
                                // avoids MASSIVE amounts of StringBuilder allocations.
                                //if (slot.tooltip.IsVisible())
                                //slot.tooltip.text = itemSlot.ToolTip();

                                slot.tooltipExtended.enabled = true;
                                if (slot.tooltipExtended.IsVisible())
                                    slot.tooltipExtended.slot = itemSlot;

                                //slot.sliderDurability.gameObject.SetActive(itemSlot.item.durability > 1);
                                slot.textDurability.text = itemSlot.item.durability > 1 ? ((int)(itemSlot.item.DurabilityPercent() * 100)).ToString() + "%" : "";
                                slot.image.sprite = itemSlot.item.image;
                                slot.image.color = Color.white; // reset for non-durability items

                                if (itemSlot.item.data is AmmoItem ammo && ammo.damageBonusFromAmmo > 0) slot.textAmmoType.text = "+ " + ammo.damageBonusFromAmmo * 100 + "%";
                                else slot.textAmmoType.text = "";

                                // cooldown if usable item
                                if (itemSlot.item.data is UsableItem usable2)
                                {
                                    float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                                    slot.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                                }
                                else slot.cooldownCircle.fillAmount = 0;

                                //if item is not used in another panel
                                if (isUsedSlot == false)
                                {
                                    slot.dragAndDropable.dragable = true;
                                    slot.dragAndDropable.dropable = xcopy < goodSlotsAmount;
                                    slot.button.interactable = xcopy < goodSlotsAmount;
                                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                                    slot.amountText.text = itemSlot.amount.ToString();
                                }
                                else
                                {
                                    slot.dragAndDropable.dragable = false;
                                    slot.dragAndDropable.dropable = false;
                                    slot.button.interactable = false;
                                    slot.amountOverlay.SetActive(false);
                                }

                                slot.button.onClick.SetListener(() =>
                                {
                                    if (player.tradingState == Player.State.sell && Input.GetKey(fastInteractHotKey))
                                    {
                                        SellSlot temp = new SellSlot();
                                        temp.index = icopy;
                                        temp.amount = itemSlot.amount;
                                        player.sellSlots.Add(temp);
                                    }
                                    else
                                    {
                                        if (itemSlot.item.data is ScriptableDurabilityRestorer restorer)
                                        {
                                            UIItemDurabilityRestoration.singleton.panel.SetActive(true);
                                            player.inventory.inventoryIndexDurabilityRestoration = slotIndex;
                                        }
                                        else if (itemSlot.item.data is UsableItem usable && usable.CanUseInventory(player, slotIndex) == Usability.Usable)
                                        {
                                            player.inventory.CmdUseItem(slotIndex);
                                        }
                                    }
                                });

                                //paint color
                                slot.GetComponent<Image>().color = xcopy < goodSlotsAmount ? colorNormalItem : colorSell;
                            }
                            else
                            {
                                slot.sliderDurability.gameObject.SetActive(false);
                                slot.imageBinding.SetActive(false);

                                // refresh items from npc trading buy
                                if (player.tradingState == Player.State.buy && slotIndex < player.npcBuying.Count && player.npcBuying[slotIndex].amount > 0)
                                {
                                    // only build tooltip while it's actually shown. this
                                    // avoids MASSIVE amounts of StringBuilder allocations.
                                    //slot.tooltip.enabled = true;
                                    //if (slot.tooltip.IsVisible())
                                    //    slot.tooltip.text = player.npcBuying[i].ToolTip();

                                    slot.tooltipExtended.enabled = true;
                                    if (slot.tooltipExtended.IsVisible())
                                        slot.tooltipExtended.slot = player.npcBuying[slotIndex];

                                    slot.dragAndDropable.dragable = false;
                                    slot.dragAndDropable.dropable = false;

                                    slot.image.color = Color.white;
                                    slot.image.sprite = player.npcBuying[slotIndex].item.image;

                                    slot.amountOverlay.SetActive(player.npcBuying[slotIndex].amount > 1);
                                    slot.amountText.text = player.npcBuying[slotIndex].amount.ToString();

                                    slot.button.interactable = true;
                                    slot.button.onClick.SetListener(() =>
                                    {
                                        //remove from inventory buying item
                                        player.CmdUpdateBuyingSlot(icopy, new ItemSlot());
                                    });

                                    //paint color
                                    if (useTradingSystemColors) slot.GetComponent<Image>().color = colorPurchasedItem;
                                }
                                else
                                {
                                    // refresh invalid item
                                    slot.button.onClick.RemoveAllListeners();
                                    slot.tooltip.enabled = false;
                                    slot.tooltipExtended.enabled = false;
                                    slot.dragAndDropable.dragable = false;
                                    slot.dragAndDropable.dropable = xcopy < goodSlotsAmount;
                                    slot.image.color = Color.clear;
                                    slot.image.sprite = null;
                                    slot.cooldownCircle.fillAmount = 0;
                                    slot.amountOverlay.SetActive(false);
                                    slot.textDurability.text = "";
                                    slot.textAmmoType.text = "";

                                    //paint color
                                    slot.GetComponent<Image>().color = xcopy < goodSlotsAmount ? colorNormalItem : colorSell;
                                }
                            }
                        }

                        countSlots += eItem.addSlots;
                        amountStorageIndex++;
                    }
                }
            }
        }
    }
}