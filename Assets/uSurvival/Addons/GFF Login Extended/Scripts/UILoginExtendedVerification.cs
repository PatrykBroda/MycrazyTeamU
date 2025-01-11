using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UILoginExtendedVerification : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private InputField inputFieldCode;
        [SerializeField] private Button buttonContinue;
        [SerializeField] private Button buttonCalcel;
        [SerializeField] private Button buttonResend;

        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton=null in Start/Awake
        public NetworkAuthenticatorSurvival auth;
        [SerializeField] private UILoginExtendedAutorization autorization;
        [SerializeField] private UILoginExtendedInfoMessages infoMessages;
        [SerializeField] private AudioSource audioSource;

        public void Show() { panel.SetActive(true); }

        private void Start()
        {
            //button continue
            buttonContinue.onClick.SetListener(() =>
            {
                infoMessages.ShowMessage("New code sent to email");

                UpdateVerificationMsg message = new UpdateVerificationMsg { account = auth.loginAccount, verification = inputFieldCode.text.ToInt(), resend = false };
                NetworkClient.Send(message);

                inputFieldCode.text = "";
            });

            //button resend code
            buttonResend.onClick.SetListener(() =>
            {
                UpdateVerificationMsg message = new UpdateVerificationMsg { account = auth.loginAccount, verification = 0, resend = true };
                NetworkClient.Send(message);
            });

            //button cancel
            buttonCalcel.onClick.SetListener(() =>
            {
                autorization.Show();
                panel.SetActive(false);
            });
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                //button continue
                buttonContinue.interactable = !string.IsNullOrEmpty(inputFieldCode.text);
            }
        }

        public void ReceiveMsgVerification(UpdateVerificationMsgtoClient message)
        {
            if (message.verificationSucces) StartCoroutine(VerificationSuccess());
            else infoMessages.ShowErrorMessage("Invalid verification code");
        }

        private IEnumerator VerificationSuccess()
        {
            infoMessages.ShowMessage("Verification complete");

            if (NetworkClient.isConnected) NetworkClient.Disconnect();
            PlayerPrefs.SetString("LastLogin", auth.loginAccount);
            PlayerPrefs.SetString("LastPass", auth.loginPassword);

            inputFieldCode.text = "";

            yield return new WaitForSeconds(infoMessages.timeInfoMessage);

            panel.SetActive(false);

            manager.StartClient();
        }
    }
}