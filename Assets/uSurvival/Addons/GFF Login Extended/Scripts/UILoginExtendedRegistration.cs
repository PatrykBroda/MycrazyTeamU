using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UILoginExtendedRegistration : MonoBehaviour
    {
        [SerializeField] private float timeout = 3;

        [SerializeField] private GameObject panel;
        [SerializeField] private InputField inputFieldAccount;
        [SerializeField] private InputField inputFieldPassword;
        [SerializeField] private InputField inputFieldEmail;
        [SerializeField] private Button buttonRegister;
        [SerializeField] private Button buttonCancel;

        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton=null in Start/Awake
        public NetworkAuthenticatorSurvival auth;
        [SerializeField] private UILoginExtendedServer server;
        [SerializeField] private UILoginExtendedAutorization autorization;
        [SerializeField] private UILoginExtendedInfoMessages infoMessages;
        [SerializeField] private AudioSource audioSource;

        public void Show(){panel.SetActive(true);}

        private void Start()
        {
            buttonRegister.onClick.SetListener(() =>
            {
                buttonRegister.interactable = false;
                StartCoroutine(StartTimeout());

                if (!manager.emailRequiredForRegistration || !string.IsNullOrWhiteSpace(inputFieldEmail.text))
                {
                    if (NetworkClient.isConnected == true)
                    {
                        RegistrationMsg message = new RegistrationMsg { account = inputFieldAccount.text, password = inputFieldPassword.text, email = inputFieldEmail.text };
                        NetworkClient.Send(message);
                    }
                    else infoMessages.ShowErrorMessage("Not Connected");
                }
                else infoMessages.ShowErrorMessage("Invalid email");
            });

            buttonCancel.onClick.SetListener(() =>
            {
                panel.SetActive(false);
                buttonRegister.interactable = true;
                autorization.Show();
            });
        }

        private IEnumerator StartTimeout()
        {
            yield return new WaitForSeconds(timeout);
            buttonRegister.interactable = true;
        }

        public void ReceiveMsgRegistrationReply(RegistrationReplyMsgtoClient message)
        {
            if (panel.activeSelf)
            {
                if (message.reply == RegistrationError.none) StartCoroutine(RegistrationSuccess());
                else
                {
                    if (message.reply == RegistrationError.accountLengthIsNotCorrect) infoMessages.ShowErrorMessage("Account must be no less " + "" + manager.accountMinLength);
                    else if (message.reply == RegistrationError.passwordLengthIsNotCorrect) infoMessages.ShowErrorMessage("Password must be no less " + "" + manager.passwordMinLength);
                    else if (message.reply == RegistrationError.loginIsNotFree) infoMessages.ShowErrorMessage("Login already in use");
                    else if (message.reply == RegistrationError.emailIsNotFree) infoMessages.ShowErrorMessage("Email already in use");
                }
            }
        }

        private IEnumerator RegistrationSuccess()
        {
            PlayerPrefs.SetString("LastLogin", inputFieldAccount.text);
            PlayerPrefs.SetString("LastPass", inputFieldPassword.text);

            infoMessages.ShowMessage("Account created successfully");
            yield return new WaitForSeconds(infoMessages.timeInfoMessage);

            autorization.Show(inputFieldAccount.text, inputFieldPassword.text);
            CancelInvoke();

            inputFieldAccount.text = "";
            inputFieldPassword.text = "";
            inputFieldEmail.text = "";

            panel.SetActive(false);
        }
    }
}