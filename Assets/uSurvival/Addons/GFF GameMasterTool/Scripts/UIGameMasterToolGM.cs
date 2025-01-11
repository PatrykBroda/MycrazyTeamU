using System;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIGameMasterToolGM : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Toggle toggleInvulnerability;
        [SerializeField] private Toggle toggleInvisibility;
        [SerializeField] private Toggle toggleMoveSpeed;
        [SerializeField] private Toggle toggleKillingWithOneHit;

        [SerializeField] private InputField inputFieldGold;
        [SerializeField] private InputField inputFieldCoins;

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    toggleInvulnerability.onValueChanged.SetListener((value) => { }); // avoid callback while setting .isOn via code
                    toggleInvulnerability.isOn = player.gameMasterTool.immortality;
                    toggleInvulnerability.onValueChanged.SetListener((value) =>
                    {
                        player.gameMasterTool.CmdSetCharacterImmortality(value);
                    });

                    toggleInvisibility.onValueChanged.SetListener((value) => { }); // avoid callback while setting .isOn via code
                    toggleInvisibility.isOn = player.gameMasterTool.invisibility;
                    toggleInvisibility.onValueChanged.SetListener((value) =>
                    {
                        player.gameMasterTool.CmdSetCharacterInvisibility(value);
                    });

                    toggleKillingWithOneHit.onValueChanged.SetListener((value) => { }); // avoid callback while setting .isOn via code
                    toggleKillingWithOneHit.isOn = player.gameMasterTool.killingWithOneHit;
                    toggleKillingWithOneHit.onValueChanged.SetListener((value) =>
                    {
                        player.gameMasterTool.CmdSetCharacterKillingWithOneHit(value);
                    });

                    toggleMoveSpeed.onValueChanged.SetListener((value) => { }); // avoid callback while setting .isOn via code
                    toggleMoveSpeed.isOn = player.gameMasterTool.superSpeed;
                    toggleMoveSpeed.onValueChanged.SetListener((value) =>
                    {
                        player.gameMasterTool.CmdSetCharacterSuperSpeed(value);
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
                }
            }
        }
    }
}