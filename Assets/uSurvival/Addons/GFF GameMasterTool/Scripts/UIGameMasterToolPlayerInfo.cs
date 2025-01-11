using System;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIGameMasterToolPlayerInfo : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private InputField playerNameInputCharacter;
        [SerializeField] private Button buttonFindCharacter;
        [SerializeField] private Text textCharacterStatusValue;
        [SerializeField] private Text textPlayerName;

        //stats
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthStatus;
        [SerializeField] private Slider hydrationSlider;
        [SerializeField] private Text hydrationStatus;
        [SerializeField] private Slider nutritionSlider;
        [SerializeField] private Text nutritionStatus;
        [SerializeField] private Slider temperatureSlider;
        [SerializeField] private Text temperatureStatus;
        [SerializeField] private string temperatureUnit = "°C";
        [SerializeField] private int temperatureDecimalDigits = 1;
        [SerializeField] private Slider enduranceSlider;
        [SerializeField] private Text enduranceStatus;

        [SerializeField] private Button buttonSetMinHealth;
        [SerializeField] private Button buttonSetMaxHealth;
        [SerializeField] private Button buttonSetMinHydration;
        [SerializeField] private Button buttonSetMaxHydration;
        [SerializeField] private Button buttonSetMinNutrition;
        [SerializeField] private Button buttonSetMaxNutrition;
        [SerializeField] private Button buttonSetMinTemperature;
        [SerializeField] private Button buttonSetMaxTemperature;
        [SerializeField] private Button buttonSetMinEndurance;
        [SerializeField] private Button buttonSetMaxEndurance;

        [SerializeField] private InputField inputFieldGold;
        [SerializeField] private InputField inputFieldCoins;

        //actions
        public Button warpButton;
        public Button summonButton;
        public Button killButton;
        public Button kickButton;
        public Button chatButton;
        public Button banButton;
        public InputField inputFieldBanReason;

        //inventory
        public Button buttonInventory;
        public GameObject panelInventoryOriginal;

        //equipment
        public Button buttonEquipment;
        public GameObject panelEquipmentOriginal;

        //hootbar
        public Button buttonHootbar;
        public GameObject panelHootbar;

        [Header("Addons")]
        public GameObject panelEquipmentExtended;
        public GameObject panelInventoryExtended;

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    buttonFindCharacter.onClick.SetListener(() =>
                    {
                        //find in online players
                        player.gameMasterTool.CmdFindPlayerByName(playerNameInputCharacter.text);
                    });

                    GameMasterMessage message = player.gameMasterTool.playerInfoMessage;

                    textPlayerName.text = message.targetName;
                    //textCharacterStatusValue.text = message.isOnline ? "Online" : "Offline";

                    buttonInventory.interactable = string.IsNullOrEmpty(message.targetName) == false;
                    buttonInventory.onClick.SetListener(() =>
                    {
                        if (panelInventoryExtended == null) panelInventoryOriginal.SetActive(true);
                        else panelInventoryExtended.SetActive(true);
                    });

                    buttonEquipment.interactable = string.IsNullOrEmpty(message.targetName) == false;
                    buttonEquipment.onClick.SetListener(() =>
                    {
                        if (panelEquipmentExtended == null) panelEquipmentOriginal.SetActive(true);
                        else panelEquipmentExtended.SetActive(true);
                    });

                    if (string.IsNullOrEmpty(message.targetName) == false)
                    {
                        // health
                        healthSlider.value = message.healthValue;
                        healthStatus.text = message.healthStatus;
                        buttonSetMinHealth.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetHealthForPlayer(message.targetName, 0);
                        });
                        buttonSetMaxHealth.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetHealthForPlayer(message.targetName, message.healthMax);
                        });

                        // hydration
                        hydrationSlider.value = message.hydrationValue;
                        hydrationStatus.text = message.hydrationStatus;
                        buttonSetMinHydration.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetHydrationForPlayer(message.targetName, 0);
                        });
                        buttonSetMaxHydration.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetHydrationForPlayer(message.targetName, message.hydrationMax);
                        });

                        // nutrition
                        nutritionSlider.value = message.nutritionValue;
                        nutritionStatus.text = message.nutritionStatus;
                        buttonSetMinNutrition.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetNutritionForPlayer(message.targetName, 0);
                        });
                        buttonSetMaxNutrition.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetNutritionForPlayer(message.targetName, message.nutritionMax);
                        });

                        // temperature (scaled down, see Temperature script)
                        temperatureSlider.value = message.temperatureValue;
                        float currentTemperature = message.temperatureCurrent / 100f;
                        float maxTemperature = message.temperatureMax / 100f;
                        string toStringFormat = "F" + temperatureDecimalDigits.ToString(); // "F2" etc.
                        temperatureStatus.text = currentTemperature.ToString(toStringFormat) + " / " +
                                                 maxTemperature.ToString(toStringFormat) + " " +
                                                 temperatureUnit;
                        buttonSetMinTemperature.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetTemperatureForPlayer(message.targetName, 0);
                        });
                        buttonSetMaxTemperature.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetTemperatureForPlayer(message.targetName, message.temperatureMax);
                        });

                        // endurance
                        enduranceSlider.value = message.enduranceValue;
                        enduranceStatus.text = message.enduranceStatus;
                        buttonSetMinEndurance.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetEnduranceForPlayer(message.targetName, 0);
                        });
                        buttonSetMaxEndurance.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSetEnduranceForPlayer(message.targetName, message.enduranceMax);
                        });

                        // gold: set if not editing, apply otherwise
                        if (!inputFieldGold.isFocused)
                            inputFieldGold.text = player.gold.ToString();

                        inputFieldGold.onEndEdit.SetListener((value) =>
                        {
                            player.gameMasterTool.CmdSetCharacterGold(Convert.ToUInt32(value));
                        });

                        // coins: set if not editing, apply otherwise
                        if (!inputFieldCoins.isFocused)
                            inputFieldCoins.text = player.itemMall.coins.ToString();
                        inputFieldCoins.onEndEdit.SetListener((value) =>
                        {
                            player.gameMasterTool.CmdSetCharacterCoins(Convert.ToUInt32(value));
                        });

                        //warp
                        warpButton.interactable = true;
                        warpButton.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdWarp(message.targetName);
                        });

                        //summon
                        summonButton.interactable = true;
                        summonButton.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdSummon(message.targetName);
                        });

                        //kill
                        killButton.interactable = true;
                        killButton.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdKill(message.targetName);
                        });

                        //kick
                        kickButton.interactable = true;
                        kickButton.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdKick(message.targetName);
                        });

                        //ban
                        banButton.interactable = true;
                        banButton.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdBan(message.targetName, inputFieldBanReason.text);
                        });

                        //chat
                        chatButton.interactable = true;
                        chatButton.onClick.SetListener(() =>
                        {
                            // set text to reply prefix
                            UIChat.singleton.messageInput.text = "/w " + message.targetName + " ";

                            // activate
                            UIChat.singleton.messageInput.Select();

                            // move cursor to end (doesn't work in here, needs small delay)
                            UIChat.singleton.messageInput.MoveTextEnd(false);

                            //UIChatExtended.singleton.messageInput.text = "/w " + message.targetName + " ";
                            //UIChatExtended.singleton.ShowForGmTool();
                        });
                    }
                    else
                    {
                        warpButton.interactable = false;
                        summonButton.interactable = false;
                        killButton.interactable = false;
                        kickButton.interactable = false;
                        banButton.interactable = false;
                        chatButton.interactable = false;

                        inputFieldGold.text = "";
                        inputFieldCoins.text = "";
                    }
                }
            }
        }

        public void SetUpPlayerToSearch(string name)
        {
            playerNameInputCharacter.text = name;
        }
    }
}