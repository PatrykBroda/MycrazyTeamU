using UnityEngine;
using UnityEngine.UI;

namespace uSurvival
{
    public partial class UIMainPanel : MonoBehaviour
    {
        // singleton to access it from player scripts without FindObjectOfType
        public static UIMainPanel singleton;

        public KeyCode hotKey = KeyCode.Tab;
        public GameObject panel;
        public Button quitButton;
        public Button quitConfirmButton;
        public GameObject panelQuitOrRestart;

        public GameObject imageInButtonFriends;

        public GameObject panelStorage;
        public GameObject panelStats;
        public GameObject panelCharacterInfo;
        public GameObject panelFriends;
        public GameObject panelMail;
        public GameObject panelGuild;
        public GameObject panelNpcTrade;

        public GameObject panelCraft;
        public GameObject panelQuests;


        public Button buttonCraft;
        public Button buttonFriends;

        public UIMainPanel()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            if (singleton == null) singleton = this;
        }

        private void Start()
        {
            quitButton.onClick.SetListener(() =>
            {
                panelQuitOrRestart.SetActive(true);
            });
            quitConfirmButton.onClick.SetListener(() =>
            {
                NetworkManagerSurvival.Quit();
            });

            buttonCraft.onClick.SetListener(() =>
            {
                panelCraft.SetActive(!panelCraft.activeSelf);
                panelMail.SetActive(false);
                panelFriends.SetActive(false);
            });
            buttonFriends.onClick.SetListener(() =>
            {
                panelFriends.SetActive(!panelFriends.activeSelf);
                panelMail.SetActive(false);
                panelCraft.SetActive(false);
            });
            buttonMail.onClick.SetListener(() =>
            {
                panelMail.SetActive(!panelMail.activeSelf);
                panelFriends.SetActive(false);
                panelCraft.SetActive(false);
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                if (player.health.current < 1 && panel.activeSelf) panel.SetActive(false);

                // hotkey (not while typing in chat, etc.)
                if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                {
                    panelStorage.SetActive(false);
                    panelNpcTrade.SetActive(false);
                    panelCraft.SetActive(false);
                    panelQuests.SetActive(false);
                    panelFriends.SetActive(false);
                    panelMail.SetActive(false);
                    panel.SetActive(!panel.activeSelf);
                }

                imageInButtonFriends.gameObject.SetActive(player.friends.friendRequests.Count > 0);

                // show "(5)Quit" if we can't log out during combat
                // -> CeilToInt so that 0.1 shows as '1' and not as '0'
                quitButton.interactable = player.remainingLogoutTime == 0;

                if (buttonMail != null)
                {
                    buttonMail.transform.GetChild(0).gameObject.SetActive(player.mail.CheckNewMailOnClient());
                }

                if (newMessagesAmount != player.mail.mail.Count)
                {
                    audioSource.PlayOneShot(clip);
                    newMessagesAmount = player.mail.mail.Count;
                }
            }
            // hide if server stopped and player disconnected
            else panel.SetActive(false);
        }

        public void Show()
        {
            panel.SetActive(true);
        }

        public void CloseAllPanels()
        {
            panelStorage.SetActive(false);
            panelCharacterInfo.SetActive(false);
            panelStats.SetActive(false);
            panelFriends.SetActive(false);
            panelMail.SetActive(false);
            panelGuild.SetActive(false);    
            panelNpcTrade.SetActive(false);

        }
    }
}