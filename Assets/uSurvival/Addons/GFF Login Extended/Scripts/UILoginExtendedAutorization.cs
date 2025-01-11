using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UILoginExtendedAutorization : MonoBehaviour
    {
        [SerializeField] private int serverTimeout = 5; //time for reconnect
        [SerializeField] private bool useCommandLineParametersForLogin = false;

        [SerializeField] private GameObject panel;
        [SerializeField] private Text textServer;
        [SerializeField] private InputField accountInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Dropdown serverDropdown;
        [SerializeField] private Toggle toggleRememberMe;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button rememberButton;
        [SerializeField] private Text textDebug;

        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton=null in Start/Awake
        public NetworkAuthenticatorSurvival auth;
        [SerializeField] private UILoginExtendedServer server;
        [SerializeField] private UILoginExtendedRegistration registration;
        [SerializeField] private UILoginExtendedVerification verification;
        [SerializeField] private UILoginExtendedAccountRecovery accountRecovery;
        [SerializeField] private UILoginExtendedInfoMessages infoMessages;

        private bool requestAutoLogin = false;
        private bool requestLoginViaVK = false;
        private int vkId = 0;
        private string vkHash;

        //singleton
        public static UILoginExtendedAutorization singleton;
        public UILoginExtendedAutorization() { singleton = this; }

        public void Initialization()
        {
            //disable the ability to select a server if the server is only one
            if (manager.serverList.Count == 1) serverDropdown.gameObject.SetActive(false);
            else
            {
                // copy servers to dropdown; copy selected one to networkmanager ip/port.
                serverDropdown.options = manager.serverList.Select(sv => new Dropdown.OptionData(sv.name)).ToList();

                // load last server by name in case order changes some day.
                if (PlayerPrefs.HasKey("LastServer"))
                {
                    string last = PlayerPrefs.GetString("LastServer", "");
                    serverDropdown.value = manager.serverList.FindIndex(s => s.name == last);
                }

                serverDropdown.onValueChanged.AddListener(delegate { CheckAnotherServer(); });
            }

            manager.networkAddress = manager.serverList[serverDropdown.value].ip;

            ClientRegisterHandles();
            ClientRegisterHandlesVkPlay();

            //start check server state
            InvokeRepeating(nameof(CheckServerStatus), 0.5f, serverTimeout);

            // load last login and password
            if (useCommandLineParametersForLogin)
            {
                vkId = CommandLineReaderVK.GetCustomArgument("--sz_pers_id").ToInt();
                vkHash = CommandLineReaderVK.GetCustomArgument("--sz_token");

                if (vkId != 0 && !string.IsNullOrEmpty(vkHash))
                {
                    textDebug.text = "Вход через VK Play";
                    requestLoginViaVK = true;
                }
                else
                {
                    textDebug.text += "\nVK Play Parametrs is null";
                    //panelTechnicalWork.SetActive(true);
                }
            }
            else
            {
                // load last login and password
                if (PlayerPrefs.HasKey("LastLogin")) accountInput.text = PlayerPrefs.GetString("LastLogin");
                if (PlayerPrefs.HasKey("LastPass")) passwordInput.text = PlayerPrefs.GetString("LastPass");
                if (PlayerPrefs.HasKey("RememberMe")) toggleRememberMe.isOn = PlayerPrefs.GetInt("RememberMe") == 0 ? false : true;

                if (toggleRememberMe.gameObject.activeSelf)
                {
                    if (toggleRememberMe.isOn)
                    {
                        if (!string.IsNullOrEmpty(accountInput.text) && !string.IsNullOrEmpty(passwordInput.text)) requestAutoLogin = true;
                        else toggleRememberMe.isOn = false;
                    }
                    toggleRememberMe.onValueChanged.AddListener(RememberMeStateChange);
                }

                //If there is no saved account, then go to the registration panel
                //if (string.IsNullOrEmpty(accountInput.text) || string.IsNullOrEmpty(passwordInput.text)) registration.Show();
                //else panel.SetActive(true);

                panel.SetActive(true);
            }
        }

        public void Show() { panel.SetActive(true); }
        public void Show(string account, string password)
        {
            accountInput.text = !string.IsNullOrEmpty(account) ? account : auth.loginAccount;
            passwordInput.text = password;
            panel.SetActive(true);
        }
        public void Hide() { panel.SetActive(false); }

        private void Start()
        {
            loginButton.onClick.SetListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(accountInput.text) && !string.IsNullOrWhiteSpace(passwordInput.text) &&
                    accountInput.text.Length >= manager.accountMinLength && accountInput.text.Length <= manager.accountMaxLength &&
                    passwordInput.text.Length >= manager.passwordMinLength)
                {
                    LoginExtendedMsg message = new LoginExtendedMsg { account = auth.loginAccount, password = auth.loginPassword, version = Application.version };
                    NetworkClient.Send(message);
                }
                else
                {
                    infoMessages.ShowErrorMessage("Invalid account or password");
                }
            });

            registerButton.onClick.SetListener(() =>
            {
                panel.SetActive(false);
                registration.Show();
            });

            rememberButton.onClick.SetListener(() =>
            {
                accountRecovery.Show();
                panel.SetActive(false);
            });
        }

        private void Update()
        {
            if (!server.IsServerPanelOpen)
            {
                //check connect State
                if (NetworkClient.isConnecting) textServer.text = "Checking server availability...";
                else if (NetworkClient.isConnected) textServer.text = "Game server is on";
                else textServer.text = "Game server is Off";
            }

            if (panel.activeSelf)
            {
                if (requestAutoLogin && NetworkClient.isConnected)
                {
                    LoginExtendedMsg message = new LoginExtendedMsg { account = auth.loginAccount, password = auth.loginPassword, version = Application.version };
                    NetworkClient.Send(message);
                }

                // inputs
                auth.loginAccount = accountInput.text;
                auth.loginPassword = passwordInput.text;

                //buttons Interactable
                loginButton.interactable = NetworkClient.isConnected;
                registerButton.interactable = NetworkClient.isConnected;
                rememberButton.interactable = (manager.verificationRequired && NetworkClient.isConnected);
            }

            //check connect State
            if (manager.state == NetworkState.Offline)
            {
                if (NetworkClient.isConnected)
                {
                    //textDebug.text += "\nПроверяем учетные данные на сервере 2";
                    if (requestAutoLogin)
                    {
                        LoginExtendedMsg message = new LoginExtendedMsg { account = auth.loginAccount, password = auth.loginPassword, version = Application.version };
                        NetworkClient.Send(message);
                    }
                    else if (requestLoginViaVK)
                    {
                        requestLoginViaVK = false;

                        LoginExtendedViaVKPlayMsg message = new LoginExtendedViaVKPlayMsg { id = vkId, OTPhash = vkHash, version = Application.version };
                        NetworkClient.Send(message);
                    }
                }
            }
        }

        public void ClientRegisterHandles()
        {
            NetworkClient.RegisterHandler<RegistrationReplyMsgtoClient>(registration.ReceiveMsgRegistrationReply, false);
            NetworkClient.RegisterHandler<TryLoginMsgtoClient>(ReceiveMsgTryLogin, false);
            NetworkClient.RegisterHandler<UpdateVerificationMsgtoClient>(verification.ReceiveMsgVerification, false);
            NetworkClient.RegisterHandler<AccountRecoveryMsgtoClient>(accountRecovery.ReceiveMsgAccountRecovery, false);
            NetworkClient.RegisterHandler<SetNewPasswordForAccountMsgtoClient>(accountRecovery.ReceiveMsgSetNewPassForAccount, false);
        }

        private void ReceiveMsgTryLogin(TryLoginMsgtoClient message)
        {
            if (panel.activeSelf)
            {
                if (message.error == LoginError.none)
                {
                    if (message.verification == 0)
                    {
                        CancelInvoke();

                        PlayerPrefs.SetString("LastLogin", auth.loginAccount);
                        PlayerPrefs.SetString("LastPass", auth.loginPassword);

                        if (NetworkClient.active)
                        {
                            NetworkClient.Disconnect();
                            NetworkClient.Shutdown();
                        }

                        manager.StartClient();
                        panel.SetActive(false);
                    }
                    else
                    {
                        panel.SetActive(false);
                        verification.Show();
                    }
                }
                else if (message.error == LoginError.outdated)
                {
                    infoMessages.ShowErrorMessage("Your client is out of date");

                    toggleRememberMe.interactable = false;
                    registerButton.interactable = false;
                }
                else if (message.error == LoginError.invalidAccountOrPassword)
                {
                    infoMessages.ShowErrorMessage("Invalid account or password");
                }
                else if (message.error == LoginError.banned)
                {
                    if (message.endBanTime.Length > 0) infoMessages.ShowErrorMessage("Account Blocked until " + message.endBanTime);
                    else infoMessages.ShowErrorMessage("Account is blocked");
                }
                else if (message.error == LoginError.alreadyLoggedIn)
                {
                    infoMessages.ShowErrorMessage("Account already logged in");
                }
            }
        }

        private void RememberMeStateChange(bool isOn)
        {
            int togglestate = toggleRememberMe.isOn == false ? 0 : 1;
            PlayerPrefs.SetInt("RememberMe", togglestate);
        }

        //CheckServerStatus
        private void CheckServerStatus()
        {
            if (manager.state == NetworkState.Offline)
            {
                if (!NetworkClient.active) NetworkClient.Connect(manager.serverList[serverDropdown.value].ip);
            }
            else CancelInvoke();
        }
        private void CheckAnotherServer()
        {
            PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);

            if (NetworkClient.active)
            {
                NetworkClient.Disconnect();
                NetworkClient.Shutdown();
            }
        }

        /* bool GetServerStatusFromIp()
     {
         System.Net.NetworkInformation.PingReply pr = p.Send(manager.serverList[serverDropdown.value].ip);
         System.Net.NetworkInformation.IPStatus status = pr.Status; //IPStatus.Success
         IPAddress ipAddr = pr.Address;
         long pingTime = pr.RoundtripTime;

         if (status == System.Net.NetworkInformation.IPStatus.Success) return true;
         else return false;
     }*/
    }
}