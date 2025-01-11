using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public partial class UIGameManagementInventory : MonoBehaviour
    {
        public GameObject panel;
        public UIInventorySlot slotPrefab;
        public Transform content;

        [Header("Durability Colors")]
        public Color brokenDurabilityColor = Color.red;
        public Color lowDurabilityColor = Color.magenta;
        [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

        // Update is called once per frame
        void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player)
                {
                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(slotPrefab.gameObject, player.gameMasterTool.playerInfoMessage.inventory.Count, content);

                    for (int i = 0; i < player.gameMasterTool.playerInfoMessage.inventory.Count; ++i)
                    {
                        UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                        ItemSlot itemSlot = player.gameMasterTool.playerInfoMessage.inventory[i];
                        if (itemSlot.amount > 0)
                        {
                            slot.image.sprite = itemSlot.item.image;
                            slot.amountOverlay.SetActive(itemSlot.amount > 1);
                            slot.amountText.text = itemSlot.amount.ToString();

                            // only build tooltip while it's actually shown. this
                            // avoids MASSIVE amounts of StringBuilder allocations.
                            slot.tooltip.enabled = true;
                            if (slot.tooltip.IsVisible())
                                slot.tooltip.text = itemSlot.ToolTip();
                            // use durability colors?
                            if (itemSlot.item.maxDurability > 0)
                            {
                                if (itemSlot.item.durability == 0)
                                    slot.image.color = brokenDurabilityColor;
                                else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                                    slot.image.color = lowDurabilityColor;
                                else
                                    slot.image.color = Color.white;
                            }
                            else slot.image.color = Color.white; // reset for non-durability items
                        }
                        else
                        {
                            slot.tooltip.enabled = false;
                            slot.image.color = Color.clear;
                            slot.image.sprite = null;
                            slot.amountOverlay.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}


