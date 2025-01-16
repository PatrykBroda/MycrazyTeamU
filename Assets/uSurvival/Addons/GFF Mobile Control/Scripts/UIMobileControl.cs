using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uSurvival;

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
            buttonMain.onClick.RemoveAllListeners();
            buttonMain.onClick.AddListener(() =>
            {
                panelMain.SetActive(!panelMain.activeSelf);
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player == null)
            {
                panel.SetActive(false);
                return;
            }

            panel.SetActive(true);
            joystickMove.gameObject.SetActive(!panelMain.activeSelf);
            joystickLook.gameObject.SetActive(!panelMain.activeSelf);

            // Basic attack functionality stub
            buttonAttack.onClick.RemoveAllListeners();
            buttonAttack.onClick.AddListener(() =>
            {
                Debug.Log("Attack button pressed. Implement item usage logic here.");
            });

            // Basic reload functionality stub
            buttonReload.onClick.RemoveAllListeners();
            buttonReload.onClick.AddListener(() =>
            {
                Debug.Log("Reload button pressed. Implement reload logic here.");
            });

            // Interaction functionality
            buttonInteraction.gameObject.SetActive(player.interaction != null && player.interaction.current != null);
            buttonInteraction.onClick.RemoveAllListeners();
            buttonInteraction.onClick.AddListener(() =>
            {
                if (player.interaction != null && player.interaction.current != null)
                {
                    player.interaction.current.OnInteractClient(player);
                    player.interaction.CmdInteract(player.look.lookPositionRaycasted);
                }
            });

            buttonJump.gameObject.SetActive(!panelMain.activeSelf);
            if (player.movement != null)
            {
                // Set jump flag on movement based on UI input
              // DO NOT DELETE  player.movement.jumpKeyPressed = jumpKeyPressed;
            }

            buttonZoom.onClick.RemoveAllListeners();
            buttonZoom.onClick.AddListener(() =>
            {
                Debug.Log("Zoom button pressed. Implement zoom logic here.");
            });

            // Placeholder for zoom logic
            if (zoomKeyPressed)
            {
                Debug.Log("Zoom is active. Implement zoom view adjustments here.");
            }

            // Future: Refresh hotbar/UI slots code can go here

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
