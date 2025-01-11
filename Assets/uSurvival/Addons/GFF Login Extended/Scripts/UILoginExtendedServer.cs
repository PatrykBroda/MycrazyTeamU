using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UILoginExtendedServer : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private InputField inputFieldAccount;
        [SerializeField] private InputField inputFieldPassword;
        [SerializeField] private Button buttonHost;
        [SerializeField] private Button buttondDedicated;
        [SerializeField] private Button buttonLogin;
        [SerializeField] private Text textsStatus;

        [Header("Colors")]
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color normalColor = Color.grey;

        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton=null in Start/Awake
        public NetworkAuthenticatorSurvival auth;

        public void Initialization(bool enableLogin)
        {
            // load last login and password
            if (PlayerPrefs.HasKey("LastLogin")) inputFieldAccount.text = PlayerPrefs.GetString("LastLogin");
            if (PlayerPrefs.HasKey("LastPass")) inputFieldPassword.text = PlayerPrefs.GetString("LastPass");

            // copy servers to dropdown; copy selected one to networkmanager ip/port.
            manager.networkAddress = "127.0.0.1";

            textsStatus.text = "Game server is turned off. \nWhat do you want to do ? ";
            panel.SetActive(true);
            buttonLogin.gameObject.SetActive(enableLogin);
        }

        public void Show() { panel.SetActive(true); }
        public void Hide() { panel.SetActive(false); }

        public bool IsServerPanelOpen => panel.activeSelf;

        private void Start()
        {
            buttonHost.onClick.SetListener(() =>
            {
                panel.SetActive(false);
                manager.StartHost();
            });

            buttondDedicated.onClick.SetListener(() =>
            {
                if (!manager.isNetworkActive)
                {
                    textsStatus.color = successColor;
                    textsStatus.text = "Server running";
                    buttondDedicated.GetComponentInChildren<Text>().text = "Stop Server";

                    manager.StartServer();
                }
                else
                {
                    buttondDedicated.GetComponentInChildren<Text>().text = "Server Only";

                    manager.StopServer();
                    textsStatus.color = normalColor;
                    textsStatus.text = "Game server is turned off. \nWhat do you want to do ? ";
                }
            });

            buttonLogin.onClick.SetListener(() =>
            {
                panel.SetActive(false);
                manager.StartClient();
            });
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                // inputs
                inputFieldAccount.interactable = !manager.isNetworkActive;
                inputFieldPassword.interactable = !manager.isNetworkActive;

                auth.loginAccount = inputFieldAccount.text;
                auth.loginPassword = inputFieldPassword.text;

                buttonHost.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive && auth.IsAllowedAccountName(inputFieldPassword.text);

                buttondDedicated.interactable = Application.platform != RuntimePlatform.WebGLPlayer;
            }
        }
    }
}