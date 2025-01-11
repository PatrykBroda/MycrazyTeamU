using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIGameMasterTool : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button buttonOpen;

        [SerializeField] private Button buttonServer;
        [SerializeField] private Button buttonAdmins;
        [SerializeField] private Button buttonPlayers;
        [SerializeField] private Button buttonCharacter;

        [SerializeField] private GameObject panelServer;
        [SerializeField] private GameObject panelAdmins;
        [SerializeField] private GameObject panelPlayers;
        [SerializeField] private GameObject panelCharacter;

        public enum GameMasterToolMode { Server, Admin, OnlinePlayers, CharacterInfo };
        private GameMasterToolMode gameMasterToolMode = GameMasterToolMode.Server;

        private void Start()
        {
            buttonOpen.onClick.SetListener(() =>
            {
                panel.SetActive(!panel.activeSelf);
            });

            buttonServer.onClick.SetListener(() =>
            {
                gameMasterToolMode = GameMasterToolMode.Server;
            });
            buttonAdmins.onClick.SetListener(() =>
            {
                gameMasterToolMode = GameMasterToolMode.Admin;
            });
            buttonPlayers.onClick.SetListener(() =>
            {
                gameMasterToolMode = GameMasterToolMode.OnlinePlayers;
            });
            buttonCharacter.onClick.SetListener(() =>
            {
                gameMasterToolMode = GameMasterToolMode.CharacterInfo;
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                buttonOpen.gameObject.SetActive(player.isGameMaster);

                if (panel.activeSelf)
                {
                    panelServer.SetActive(gameMasterToolMode == GameMasterToolMode.Server);
                    panelAdmins.SetActive(gameMasterToolMode == GameMasterToolMode.Admin);
                    panelPlayers.SetActive(gameMasterToolMode == GameMasterToolMode.OnlinePlayers);
                    panelCharacter.SetActive(gameMasterToolMode == GameMasterToolMode.CharacterInfo);

                    buttonServer.interactable = gameMasterToolMode != GameMasterToolMode.Server;
                    buttonAdmins.interactable = gameMasterToolMode != GameMasterToolMode.Admin;
                    buttonPlayers.interactable = gameMasterToolMode != GameMasterToolMode.OnlinePlayers;
                    buttonCharacter.interactable = gameMasterToolMode != GameMasterToolMode.CharacterInfo;
                }
            }
            else panel.SetActive(false);
        }

        public void SetViewMode(GameMasterToolMode mode)
        {
            gameMasterToolMode = mode;
        }
    }
}