using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public class UILoginExtended : MonoBehaviour
    {
        [SerializeField] private WorkMode workMode = WorkMode.server;
        [SerializeField] private GameObject mainPanel;

        [Header("Components")]
        [SerializeField] private NetworkManagerSurvival manager; // singleton=null in Start/Awake
        [SerializeField] private UILoginExtendedServer server;
        [SerializeField] private UILoginExtendedRegistration registration;
        [SerializeField] private UILoginExtendedAutorization autorization;
        [SerializeField] private UILoginExtendedVerification verification;
        [SerializeField] private UILoginExtendedAccountRecovery recovery;
        [SerializeField] private UILoginExtendedQuitAndRestart quitAndRestart;

        private void OnEnable()
        {
#if UNITY_SERVER
            workMode = WorkMode.server;
#endif
        }

        private void Start()
        {
            if (workMode == WorkMode.server)
            {
                server.Initialization(false);
                autorization.Hide();
            }
            else if (workMode == WorkMode.client)
            {
                autorization.Initialization();
                server.Hide();
            }
            else
            {
                server.Initialization(true);
                autorization.Hide();
            }
        }

        private void Update()
        {
            mainPanel.SetActive(manager.state == NetworkState.Offline);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!manager)manager = FindObjectOfType<NetworkManagerSurvival>();

            if (manager)
            {
                if (!server.manager)server.manager = manager;
                server.auth = (NetworkAuthenticatorSurvival)manager.authenticator;

                autorization.manager = manager;
                autorization.auth = (NetworkAuthenticatorSurvival)manager.authenticator;

                verification.manager = manager;
                verification.auth = (NetworkAuthenticatorSurvival)manager.authenticator;

                registration.manager = manager;
                registration.auth = (NetworkAuthenticatorSurvival)manager.authenticator;

                recovery.manager = manager;

                quitAndRestart.manager = manager;
            }
        }
#endif
    }
}