using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIItemBinding : MonoBehaviour
    {
        public GameObject panel;
        public UniversalSlot slot;
        public Button buttonBind;
        public Text textInButton;

        //singleton
        public static UIItemBinding singleton;
        public UIItemBinding()
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
                    //item
                    if (player.inventory.itemIndexForBind != -1 && player.inventory.slots[player.inventory.itemIndexForBind].amount > 0)
                    {
                        ItemSlot itemSlot = player.inventory.slots[player.inventory.itemIndexForBind];
                        slot.image.color = Color.white;
                        slot.image.sprite = itemSlot.item.image;
                        slot.imageBinding.SetActive(itemSlot.item.binding);

                        // refresh valid item
                        slot.tooltip.enabled = true;
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = itemSlot.ToolTip();

                        slot.button.onClick.SetListener(() => {
                            player.inventory.itemIndexForBind = -1;
                        });

                        textInButton.text = itemSlot.item.binding ? Localization.Translate("Free") : Localization.Translate("Bind");
                        slot.sliderDurability.gameObject.SetActive(true);
                        slot.sliderDurability.value = itemSlot.item.DurabilityPercent();
                    }
                    else
                    {
                        slot.sliderDurability.gameObject.SetActive(false);
                        player.inventory.itemIndexForBind = -1;

                        // refresh invalid item
                        slot.button.onClick.RemoveAllListeners();
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.dragAndDropable.dropable = true;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(false);
                        slot.imageBinding.SetActive(false);

                        textInButton.text = Localization.Translate("Bind");
                    }

                    buttonBind.onClick.SetListener(() => {
                        player.inventory.CmdBindItem(player.inventory.itemIndexForBind);
                        panel.SetActive(false);
                        player.inventory.itemIndexForBind = -1;
                    });
                }
                else panel.SetActive(false);
            }
        }
    }
}