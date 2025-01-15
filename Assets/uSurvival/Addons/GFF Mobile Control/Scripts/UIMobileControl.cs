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

        public Button buttonMain;
        public GameObject panelMain;

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
            buttonMain.onClick.SetListener(() =>
            {
                panelMain.SetActive(!panelMain.activeSelf);
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                panel.SetActive(true);
                joystickMove.gameObject.SetActive(!panelMain.activeSelf);
                joystickLook.gameObject.SetActive(!panelMain.activeSelf);

                buttonAttack.onClick.SetListener(() =>
                {
                    player.hotbar.TryUseItem(player.hotbar.GetCurrentUsableItemOrHands());
                });

                buttonReload.onClick.SetListener(() =>
                {
                    if (player.health.current > 0 && player.movement.state != MoveState.CLIMBING && player.reloading.ReloadTimeRemaining() == 0)
                    {
                        // usable item in selected hotbar slot?
                        ItemSlot slot = player.hotbar.slots[player.hotbar.selection];
                        if (slot.amount > 0 && slot.item.data is RangedWeaponItem itemData)
                        {
                            // requires ammo and not fully loaded yet?
                            if (itemData.requiredAmmo != null && slot.item.ammo < itemData.magazineSize)
                            {
                                // ammo type in inventory?
                                int inventoryIndex = player.inventory.GetItemIndexByName(itemData.requiredAmmo.name);
                                if (inventoryIndex != -1)
                                {
                                    // ask server to reload
                                    player.reloading.CmdReloadWeaponOnHotbar(inventoryIndex, player.hotbar.selection);

                                    // play audio locally to avoid server delay and to save bandwidth
                                    if (itemData.reloadSound) player.reloading.audioSource.PlayOneShot(itemData.reloadSound);
                                }
                            }
                        }
                    }
                });

                buttonInteraction.gameObject.SetActive(player.interaction != null && player.interaction.current != null);
                buttonInteraction.onClick.SetListener(() =>
                {
                    // call OnInteract on client and server
                    // (some effects like doors are server sided, some effects like
                    //  'open storage UI' are client sided)
                    player.interaction.current.OnInteractClient(player);
                    player.interaction.CmdInteract(player.look.lookPositionRaycasted);
                });

                buttonJump.gameObject.SetActive(!panelMain.activeSelf);

                player.movement.jumpKeyPressed = jumpKeyPressed;

                buttonZoom.onClick.SetListener(() =>
                {

                });

                UsableItem itemData = player.hotbar.GetCurrentUsableItemOrHands();
                if (zoomKeyPressed && itemData is RangedWeaponItem)
                {
                  //  player.zoom.AssignFieldOfView(player.zoom.defaultFieldOfView - ((RangedWeaponItem)itemData).zoom);
                }
                // otherwise reset field of view
             //   else player.zoom.AssignFieldOfView(player.zoom.defaultFieldOfView);

                //gff zoom
                //player.GetComponent<Zoom>().SetZoomState();

                // refresh all
                for (int i = 0; i < slots.Length; ++i)
                {
                    UIHotbarSlot slot = slots[i].GetComponent<UIHotbarSlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index

                    ItemSlot itemSlot = player.hotbar.slots[i];

                    if (itemSlot.amount > 0)
                    {
                        // refresh valid item
                        slot.tooltip.enabled = true;
                        slot.dragAndDropable.dragable = true;
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
                        else slot.image.color = Color.white; // reset for no-durability items
                        slot.image.sprite = itemSlot.item.image;
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = itemSlot.ToolTip();
                        // cooldown if usable item
                        if (itemSlot.item.data is UsableItem)
                        {
                            UsableItem usable = (UsableItem)itemSlot.item.data;
                            float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                            slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                        }
                        else slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(itemSlot.amount > 1);
                        if (itemSlot.amount > 1) slot.amountText.text = itemSlot.amount.ToString();
                    }
                    else
                    {
                        // refresh invalid item
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(false);
                    }
                }
            }
            else panel.SetActive(false);
        }

        public void OnJumpPointerDown()
        {
            jumpKeyPressed = true;
        }

        public void OnJumpPointerUp()
        {
            jumpKeyPressed = false;
        }

        public void OnZoomPointerDown()
        {
            zoomKeyPressed = true;
        }

        public void OnZoomPointerUp()
        {
            zoomKeyPressed = false;
        }
    }
}