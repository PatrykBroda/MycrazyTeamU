using GFFAddons;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GFFAddons
{
    public class UIMobileControl : MonoBehaviour
    {
        public VariableJoystick joystickMove;
        public VariableJoystick joystickLook;

        public GameObject panel;
        public Transform[] slots;
        public Transform[] pockets;

        public Button buttonAttack;
        public Button buttonReload;
        public Button buttonInteraction;
        public Button buttonJump;
        public Button buttonCrawl;
        public Button buttonZoom;

        [Header("Durability Colors")]
        public Color brokenDurabilityColor = Color.red;
        public Color lowDurabilityColor = Color.magenta;
        [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

        public bool jumpKeyPressed;
        public bool zoomKeyPressed;

        public TextMeshProUGUI textDebug;

        private void Start()
        {
            // Initialize button listeners if assigned
            if (buttonAttack != null)
            {
                buttonAttack.onClick.AddListener(() =>
                {
                    Player player = Player.localPlayer;
                    if (player != null)
                    {
                        player.hotbar.TryUseItem(player.hotbar.GetCurrentUsableItemOrHands());
                    }
                });
            }

            if (buttonReload != null)
            {
                buttonReload.onClick.AddListener(() =>
                {
                    Player player = Player.localPlayer;
                    if (player != null)
                    {
                        ReloadWeapon(player);
                    }
                });
            }

            if (buttonZoom != null)
            {
                buttonZoom.onClick.AddListener(() =>
                {
                    zoomKeyPressed = !zoomKeyPressed;
                });
            }
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                // Activate gameplay panel
                panel.SetActive(true);

                // Ensure joysticks are always visible
                joystickMove.gameObject.SetActive(true);
                joystickLook.gameObject.SetActive(true);

                // Update jump key state
                player.movement.jumpKeyPressed = jumpKeyPressed;

                // Handle Zoom functionality
                UsableItem itemData = player.hotbar.GetCurrentUsableItemOrHands();
                if (zoomKeyPressed && itemData is RangedWeaponItem rangedItem)
                {
                  //  player.zoom.AssignFieldOfView(player.zoom.defaultFieldOfView - rangedItem.zoom);
                }
                else
                {
                  //  player.zoom.AssignFieldOfView(player.zoom.defaultFieldOfView);
                }

                // Refresh hotbar slots
                RefreshHotbarSlots(player);
            }
            else
            {
                // Hide gameplay panel if player is not available
                panel.SetActive(false);
            }
        }

        private void RefreshHotbarSlots(Player player)
        {
            for (int i = 0; i < slots.Length; ++i)
            {
                UIHotbarSlot slot = slots[i].GetComponent<UIHotbarSlot>();
                slot.dragAndDropable.name = i.ToString(); // Drag-and-drop index

                ItemSlot itemSlot = player.hotbar.slots[i];

                if (itemSlot.amount > 0)
                {
                    // Refresh valid item
                    slot.tooltip.enabled = true;
                    slot.dragAndDropable.dragable = true;

                    // Use durability colors
                    if (itemSlot.item.maxDurability > 0)
                    {
                        if (itemSlot.item.durability == 0)
                            slot.image.color = brokenDurabilityColor;
                        else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                            slot.image.color = lowDurabilityColor;
                        else
                            slot.image.color = Color.white;
                    }
                    else
                    {
                        slot.image.color = Color.white; // Reset for no-durability items
                    }

                    slot.image.sprite = itemSlot.item.image;

                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();

                    // Cooldown for usable items
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else
                    {
                        slot.cooldownCircle.fillAmount = 0;
                    }

                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    if (itemSlot.amount > 1)
                        slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    // Refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                }
            }
        }

        private void ReloadWeapon(Player player)
        {
            if (player.health.current > 0 && player.movement.state != MoveState.CLIMBING && player.reloading.ReloadTimeRemaining() == 0)
            {
                ItemSlot slot = player.hotbar.slots[player.hotbar.selection];
                if (slot.amount > 0 && slot.item.data is RangedWeaponItem itemData)
                {
                    if (itemData.requiredAmmo != null && slot.item.ammo < itemData.magazineSize)
                    {
                        int inventoryIndex = player.inventory.GetItemIndexByName(itemData.requiredAmmo.name);
                        if (inventoryIndex != -1)
                        {
                            player.reloading.CmdReloadWeaponOnHotbar(inventoryIndex, player.hotbar.selection);
                            if (itemData.reloadSound) player.reloading.audioSource.PlayOneShot(itemData.reloadSound);
                        }
                    }
                }
            }
        }

        // UI Button Event Handlers
        public void OnJumpPointerDown() => jumpKeyPressed = true;

        public void OnJumpPointerUp() => jumpKeyPressed = false;

        public void OnZoomPointerDown() => zoomKeyPressed = true;

        public void OnZoomPointerUp() => zoomKeyPressed = false;
    }
}
