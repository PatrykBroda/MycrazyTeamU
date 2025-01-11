using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UILoginExtendedAccountRecovery : MonoBehaviour
    {
        [SerializeField] private GameObject panelRecoverAccountRequest;
        [SerializeField] private GameObject panelRecoverAccountEnterCode;

        [Header("Account Recovery Request")]
        [SerializeField] private InputField inputEmailForRecover;
        [SerializeField] private InputField inputCodeForRecoverRequest;
        [SerializeField] private Text textCodeVerification;
        [SerializeField] private Button buttonRequestRecover;
        [SerializeField] private Button buttonCancelRecover;

        [Header("Recover Account set new Password")]
        [SerializeField] private InputField inputCodeVerification;
        [SerializeField] private InputField inputNewPassword;
        [SerializeField] private Button buttonSetupNewPassword;
        [SerializeField] private Button buttonCancelSetupNewPassword;

        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton=null in Start/Awake
        [SerializeField] private UILoginExtendedAutorization autorization;
        [SerializeField] private UILoginExtendedInfoMessages infoMessages;
        [SerializeField] private AudioSource audioSource;

        public void Show()
        {
            textCodeVerification.text = Random.Range(10000, 99999).ToString();
            panelRecoverAccountRequest.SetActive(true);
        }

        private void Start()
        {
            //button recovery
            buttonRequestRecover.onClick.SetListener(() =>
            {
                if (textCodeVerification.text == inputCodeForRecoverRequest.text)
                {
                    if (inputEmailForRecover.text != "")
                    {
                        infoMessages.ShowMessage("Information sent to the Email");

                        AccountRecoveryMsg message = new AccountRecoveryMsg { email = inputEmailForRecover.text };
                        NetworkClient.Send(message);
                    }
                    else infoMessages.ShowErrorMessage("Email not found");
                }
                else infoMessages.ShowErrorMessage("Invalid verification code");
            });

            //button cancel
            buttonCancelRecover.onClick.SetListener(() =>
            {
                autorization.Show();
                panelRecoverAccountRequest.SetActive(false);
            });

            buttonSetupNewPassword.onClick.SetListener(() =>
            {
                SetNewPasswordForAccountMsg message = new SetNewPasswordForAccountMsg { code = inputCodeVerification.text.ToInt(), email = inputEmailForRecover.text, password = inputNewPassword.text };
                NetworkClient.Send(message);
            });

            //button recovery account cancel 
            buttonCancelSetupNewPassword.onClick.SetListener(() =>
            {
                autorization.Show();
                panelRecoverAccountRequest.SetActive(false);
            });
        }

        private void Update()
        {
            //button recovery account
            buttonRequestRecover.gameObject.SetActive(manager.verificationRequired);
        }

        public void ReceiveMsgAccountRecovery(AccountRecoveryMsgtoClient message)
        {
            if (panelRecoverAccountRequest.activeSelf)
            {
                if (message.findEmail) infoMessages.ShowMessage("Information sent to the Email");
                else infoMessages.ShowErrorMessage("Email not found");
            }
        }

        public void ReceiveMsgSetNewPassForAccount(SetNewPasswordForAccountMsgtoClient message)
        {
            if (panelRecoverAccountEnterCode.activeSelf)
            {
                if (message.result)
                {
                    infoMessages.ShowMessage("Information sent to the Email");
                    autorization.Show("", inputNewPassword.text);
                    StartCoroutine(DisablePanelSetNewPassForAccount());
                }
                else infoMessages.ShowErrorMessage("Email not found");
            }
        }

        private IEnumerator DisablePanelSetNewPassForAccount()
        {
            yield return new WaitForSeconds(infoMessages.timeInfoMessage);
            panelRecoverAccountEnterCode.SetActive(false);
            autorization.Show();
        }
    }
}