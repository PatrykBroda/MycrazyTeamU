// UIMobileControl.cs

using GFFAddons;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;
using Mirror; // Ensure you're using Mirror or the appropriate networking framework

namespace uSurvival
{
    // Ensure that PlayerMovement inherits from NetworkBehaviour to override OnStartLocalPlayer
    public partial class PlayerMovement : NetworkBehaviour
    {
        public VariableJoystick joystick;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer(); // Always call the base method when overriding
            joystick = GameObject.FindWithTag("Joystick").GetComponent<UIMobileControl>().joystickMove;
        }
    }

    // Ensure that PlayerLook inherits from NetworkBehaviour to override OnStartLocalPlayer
    public partial class PlayerLook : NetworkBehaviour
    {
        public VariableJoystick joystick;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer(); // Always call the base method when overriding
            joystick = GameObject.FindWithTag("Joystick").GetComponent<UIMobileControl>().joystickLook;
        }
    }

    // Ensure that Player inherits from NetworkBehaviour and includes a Zoom member
    public partial class Player : NetworkBehaviour
    {
        public Zoom zoom;

        // Initialize the Zoom component
        private void Awake()
        {
            zoom = GetComponent<Zoom>();
            if (zoom == null)
            {
                Debug.LogError("Zoom component is missing on the Player GameObject.");
            }
        }

        // Assuming you have a static reference to the local player
        public static Player localPlayer;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            localPlayer = this;
        }
    }

    // Definition of Zoom class (Ensure this is in a separate file or within the same namespace)
    public class Zoom : MonoBehaviour
    {
        public float defaultFieldOfView = 60f;
        private Camera playerCamera;

        private void Start()
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = defaultFieldOfView;
            }
            else
            {
                Debug.LogError("Main Camera not found.");
            }
        }

        public void AssignFieldOfView(float fov)
        {
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = Mathf.Clamp(fov, 20f, 100f); // Clamp to reasonable FOV values
            }
        }
    }
}

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
            // Ensure all buttons are assigned
            if (buttonMain != null)
            {
                buttonMain.onClick.AddListener(() =>
                {
                    panelMain.SetActive(!panelMain.activeSelf);
                });
            }
            else
            {
                Debug.LogError("buttonMain is not assigned in the inspector.");
            }

            // Initialize other buttons similarly if needed
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                panel.SetActive(true);
                joystickMove.gameObject.SetActive(!panelMain.activeSelf);
                joystickLook.gameObject.SetActive(!panelMain.activeSelf);

                // Ensure buttons are assigned before adding listeners
                if (buttonAttack != null)
                {
                    // Remove previous listeners to prevent multiple assignments
                    buttonAttack.onClick.RemoveAllListeners();
                    buttonAttack.onClick.AddListener(() =>
                    {
                        player.hotbar.TryUseItem(player.hotbar.GetCurrentUsableItemOrHands());
                    });
                }

                if (buttonReload != null)
                {
                    buttonReload.onClick.RemoveAllListeners();
                    buttonReload.onClick.AddListener(() =>
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
                }

                if (buttonInteraction != null)
                {
                    buttonInteraction.gameObject.SetActive(player.interaction != null && player.interaction.current != null);
                    buttonInteraction.onClick.RemoveAllListeners();
                    buttonInteraction.onClick.AddListener(() =>
                    {
                        // call OnInteract on client and server
                        // (some effects like doors are server sided, some effects like
                        //  'open storage UI' are client sided)
                        player.interaction.current.OnInteractClient(player);
                        player.interaction.CmdInteract(player.look.lookPositionRaycasted);
                    });
                }

                if (buttonJump != null)
                {
                    buttonJump.gameObject.SetActive(!panelMain.activeSelf);
                }

                if (buttonZoom != null)
                {
                    buttonZoom.onClick.RemoveAllListeners();
                    buttonZoom.onClick.AddListener(() =>
                    {
                        // Toggle zoom state
                        zoomKeyPressed = !zoomKeyPressed;
                    });
                }

                // Update jump key state
                player.movement.jumpKeyPressed = jumpKeyPressed;

                // Handle Zoom functionality
                UsableItem itemData = player.hotbar.GetCurrentUsableItemOrHands();
                if (zoomKeyPressed && itemData is RangedWeaponItem rangedItem)
                {
                  //here  player.zoom.AssignFieldOfView(player.zoom.defaultFieldOfView - rangedItem.zoom);
                }
                else
                {
                  //here  player.zoom.AssignFieldOfView(player.zoom.defaultFieldOfView);
                }

                // Refresh all hotbar slots
                for (int i = 0; i < slots.Length; ++i)
                {
                    UIHotbarSlot slot = slots[i].GetComponent<UIHotbarSlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index

                    ItemSlot itemSlot = player.hotbar.slots[i];

                    if (itemSlot.amount > 0)
                    {
                        // Refresh valid item
                        slot.tooltip.enabled = true;
                        slot.dragAndDropable.dragable = true;

                        // Use durability colors?
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
                            slot.image.color = Color.white; // reset for no-durability items
                        }

                        slot.image.sprite = itemSlot.item.image;

                        // Only build tooltip while it's actually shown. This
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = itemSlot.ToolTip();

                        // Cooldown if usable item
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
            else
            {
                panel.SetActive(false);
            }
        }

        // UI Button Event Handlers
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
