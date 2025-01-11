using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIItemDurabilityRestoration : MonoBehaviour
    {
        public GameObject panel;
        public UniversalSlot slotItem;
        public UniversalSlot slotRepairKit;
        public Button buttonRestore;
        public Text textDurabilityValue;

        //singleton
        public static UIItemDurabilityRestoration singleton;
        public UIItemDurabilityRestoration()
        {
            singleton = this;
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null && player.health.current > 0)
                {
                    ItemSlot itemSlot = new ItemSlot();
                    if (player.inventory.indexRecoveredItem != -1 && player.inventory.slots[player.inventory.indexRecoveredItem].amount > 0) itemSlot = player.inventory.slots[player.inventory.indexRecoveredItem];
                    else if (player.equipment.indexRecoveredItem != -1 && player.equipment.slots[player.equipment.indexRecoveredItem].amount > 0) itemSlot = player.equipment.slots[player.equipment.indexRecoveredItem];

                    //slot 1
                    if (itemSlot.amount > 0)
                    {
                        slotItem.image.color = Color.white;
                        slotItem.image.sprite = itemSlot.item.image;
                        slotItem.imageBinding.SetActive(itemSlot.item.binding);

                        // refresh valid item
                        slotItem.tooltip.enabled = true;
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        if (slotItem.tooltip.IsVisible())
                            slotItem.tooltip.text = itemSlot.ToolTip();

                        slotItem.button.onClick.SetListener(() => {
                            player.inventory.indexRecoveredItem = -1;
                            player.equipment.indexRecoveredItem = -1;
                        });

                        slotItem.sliderDurability.gameObject.SetActive(false);
                        //slotItem.sliderDurability.value = itemSlot.item.DurabilityPercent();
                        textDurabilityValue.text = Localization.Translate("Durability") + ": " + (itemSlot.item.DurabilityPercent() * 100).ToString("F2") + "%";
                    }
                    else
                    {
                        slotItem.sliderDurability.gameObject.SetActive(false);
                        player.inventory.indexRecoveredItem = -1;

                        // refresh invalid item
                        slotItem.button.onClick.RemoveAllListeners();
                        slotItem.tooltip.enabled = false;
                        slotItem.dragAndDropable.dragable = false;
                        slotItem.dragAndDropable.dropable = true;
                        slotItem.image.color = Color.clear;
                        slotItem.image.sprite = null;
                        slotItem.cooldownCircle.fillAmount = 0;
                        slotItem.amountOverlay.SetActive(false);
                        slotItem.imageBinding.SetActive(false);

                        textDurabilityValue.text = "";
                    }

                    //item
                    if (player.inventory.inventoryIndexDurabilityRestoration != -1 && player.inventory.slots[player.inventory.inventoryIndexDurabilityRestoration].amount > 0)
                    {
                        ItemSlot itemSlot2 = player.inventory.slots[player.inventory.inventoryIndexDurabilityRestoration];
                        slotRepairKit.image.color = Color.white;
                        slotRepairKit.image.sprite = itemSlot2.item.image;
                        slotRepairKit.imageBinding.SetActive(itemSlot2.item.binding);

                        // refresh valid item
                        slotRepairKit.tooltip.enabled = true;
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        if (slotRepairKit.tooltip.IsVisible())
                            slotRepairKit.tooltip.text = itemSlot2.ToolTip();

                        slotRepairKit.button.onClick.SetListener(() => {
                            player.inventory.inventoryIndexDurabilityRestoration = -1;
                        });

                        slotRepairKit.amountOverlay.SetActive(false);
                        slotRepairKit.sliderDurability.gameObject.SetActive(false);
                        //slotRepairKit.sliderDurability.value = itemSlot.item.DurabilityPercent();
                    }
                    else
                    {
                        slotRepairKit.sliderDurability.gameObject.SetActive(false);
                        player.inventory.inventoryIndexDurabilityRestoration = -1;

                        // refresh invalid item
                        slotRepairKit.button.onClick.RemoveAllListeners();
                        slotRepairKit.tooltip.enabled = false;
                        slotRepairKit.dragAndDropable.dragable = false;
                        slotRepairKit.dragAndDropable.dropable = true;
                        slotRepairKit.image.color = Color.clear;
                        slotRepairKit.image.sprite = null;
                        slotRepairKit.cooldownCircle.fillAmount = 0;
                        slotRepairKit.amountOverlay.SetActive(false);
                        slotRepairKit.imageBinding.SetActive(false);
                    }

                    buttonRestore.onClick.SetListener(() => {
                        if (itemSlot.amount > 0 && itemSlot.item.durability < itemSlot.item.data.maxDurability)
                        {
                            if (player.inventory.indexRecoveredItem != -1)
                                player.inventory.CmdDurabilityRestore(player.inventory.inventoryIndexDurabilityRestoration, player.inventory.indexRecoveredItem);
                            else player.equipment.CmdDurabilityRestore(player.inventory.inventoryIndexDurabilityRestoration, player.equipment.indexRecoveredItem);

                            player.inventory.indexRecoveredItem = -1;
                            player.equipment.indexRecoveredItem = -1;
                            player.inventory.inventoryIndexDurabilityRestoration = -1;
                        }
                        else
                        {

                        }
                    });
                }
            }
        }
    }
}


