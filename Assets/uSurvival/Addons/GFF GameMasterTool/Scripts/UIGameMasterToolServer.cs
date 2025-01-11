using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIGameMasterToolServer : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text connectionsText;
        [SerializeField] private Text maxConnectionsText;
        [SerializeField] private Text onlinePlayerText;
        [SerializeField] private Text uptimeText;
        [SerializeField] private Text tickRateText;
        [SerializeField] private Button shutdownButton;
        [SerializeField] private InputField globalChatInput;
        [SerializeField] private Button globalChatSendButton;
        [SerializeField] private InputField globalInfoInput;
        [SerializeField] private Button globalInfoSendButton;

        [Header("Confirmation")]
        [SerializeField] private GameObject panelConfirmation;
        [SerializeField] private Text messageText;
        [SerializeField] private Button confirmButton;

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    connectionsText.text = player.gameMasterTool.connections.ToString();
                    maxConnectionsText.text = player.gameMasterTool.maxConnections.ToString();
                    onlinePlayerText.text = player.gameMasterTool.players.Count.ToString();
                    uptimeText.text = Mirror.Utils.PrettySeconds(player.gameMasterTool.uptime);
                    tickRateText.text = player.gameMasterTool.tickRate.ToString();

                    // shutdown
                    shutdownButton.onClick.SetListener(() =>
                    {
                        ConfirmationShow("Are you sure to shut down the server?", () =>
                        {
                            player.gameMasterTool.CmdShutdown();
                        });
                    });

                    // global chat
                    globalChatSendButton.interactable = !string.IsNullOrWhiteSpace(globalChatInput.text);
                    globalChatSendButton.onClick.SetListener(() =>
                    {
                        player.gameMasterTool.CmdSendGlobalChatMessage(globalChatInput.text);
                        globalChatInput.text = string.Empty;
                    });

                    // global info
                    globalInfoSendButton.interactable = !string.IsNullOrWhiteSpace(globalInfoInput.text);
                    globalInfoSendButton.onClick.SetListener(() =>
                    {
                        player.gameMasterTool.CmdSendGlobalInfoMessage(globalInfoInput.text);
                        globalInfoInput.text = string.Empty;
                    });
                }
            }
        }

        private void ConfirmationShow(string message, UnityAction onConfirm)
        {
            messageText.text = message;
            confirmButton.onClick.SetListener(onConfirm);
            panelConfirmation.SetActive(true);
        }
    }
}