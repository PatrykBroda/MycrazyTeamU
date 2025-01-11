using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIMenuHelper : MonoBehaviour
    {
        //public KeyCode hotKey = KeyCode.Escape;

        [Header("Features")]
        public bool customization;
        public bool gameControl;
        public bool restart;

        [Header("Exit and Restart")]
        public float exitTime = 5;
        [Tooltip("Required for restart.")] public NetworkManagerSurvival manager;

        [Header("Sounds & Music")]
        public AudioSource audioSource;
        public UISoundManager soundManager;

        [Header("UI Elements")]
        public GameObject panel;
        public GameObject prefab;
        public Transform content;

        //panel info
        public GameObject panelInfo;
        public Text textInfo;

        [Serializable]
        public class MenuList
        {
            public string name;
            public GameObject panel;
            public Sprite image;
            public KeyCode defaultKey;
            [HideInInspector] public KeyCode currentKey;
            public GameObject[] openAdditionalPanels = new GameObject[] { };
        }
        public List<MenuList> menuList = new List<MenuList>();

        [Header("")]
        public Button buttonCustomization;
        public GameObject panelCustomization;

        public Button buttonGameControl;
        public GameObject panelGameControl;

        public Button buttonOptions;
        public GameObject panelOptions;

        public Button restartButton;
        public Button quitButton;

        bool restartRequest;
        double restartTime;

        bool quitRequest;
        double quitTime;

        private void Start()
        {
            LoadKeysFromPlayerPrefs();

            //set Features
            buttonCustomization.gameObject.SetActive(customization);
            buttonGameControl.gameObject.SetActive(gameControl);
            restartButton.gameObject.SetActive(restart);

            buttonCustomization.onClick.AddListener(() =>
            {
                if (!restartRequest && !quitRequest)
                {
                    soundManager.PlaySoundButtonClick();
                    panel.SetActive(false);
                }
            });
            buttonGameControl.onClick.AddListener(() =>
            {
                if (!restartRequest && !quitRequest)
                {
                    soundManager.PlaySoundButtonClick();
                    panelGameControl.SetActive(true);
                    panel.SetActive(false);
                }
            });
            buttonOptions.onClick.AddListener(() =>
            {
                if (!restartRequest && !quitRequest)
                {
                    soundManager.PlaySoundButtonClick();
                    panelOptions.SetActive(!panelOptions.activeSelf);
                    panel.SetActive(false);
                }
            });
            restartButton.onClick.AddListener(() =>
            {
                if (!restartRequest && !quitRequest)
                {
                    soundManager.PlaySoundButtonClick();
                    panelInfo.SetActive(true);
                    restartButton.interactable = false;
                    quitButton.interactable = false;

                    restartRequest = true;
                    restartTime = NetworkTime.time + exitTime;
                }
            });
            quitButton.onClick.AddListener(() =>
            {
                if (!restartRequest && !quitRequest)
                {
                    soundManager.PlaySoundButtonClick();
                    panelInfo.SetActive(true);
                    restartButton.interactable = false;
                    quitButton.interactable = false;

                    quitRequest = true;
                    quitTime = NetworkTime.time + exitTime;
                }
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (!player) return;

            //Escape key will open or close the panels
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (panel.activeSelf)
                {
                    panel.SetActive(false);
                    panelOptions.SetActive(false);
                }
                else
                {
                    bool check = true;
                    for (int i = 0; i < menuList.Count; i++)
                        if (menuList[i].panel != null && menuList[i].panel.activeSelf)
                        {
                            menuList[i].panel.SetActive(false);
                            check = false;
                        }

                    panel.SetActive(check);
                }
            }

            //keys
            for (int i = 0; i < menuList.Count; ++i)
            {
                if (Input.GetKeyDown(menuList[i].currentKey) && !UIUtils.AnyInputActive())
                {
                    if (menuList[i].panel != null) menuList[i].panel.SetActive(!menuList[i].panel.activeSelf);
                }
            }

            if (restartRequest)
            {
                if (restartTime - NetworkTime.time > 1.5f)
                {
                    textInfo.text = "Restart in " + (int)(restartTime - NetworkTime.time) + " seconds";
                }
                else if (restartTime > NetworkTime.time && restartTime - NetworkTime.time <= 1.5f)
                {
                    textInfo.text = "Load Characters";
                }
                else
                {
                    restartRequest = false;

                    restartButton.interactable = true;
                    quitButton.interactable = true;
                    panel.SetActive(false);
                    panelInfo.SetActive(false);
                    textInfo.text = "";

                    //if used mounts extended addon
                    //player.mountsExtended.CmdMountUnSummonThenQuit();

                    //NetworkClient.Disconnect();
                    //NetworkClient.Shutdown();
                    manager.StopClient();

                    manager.StartClient();
                }
            }

            if (quitRequest)
            {
                if (quitTime >= NetworkTime.time)
                {
                    textInfo.text = "Quit the game after " + (int)(quitTime - NetworkTime.time) + " seconds";
                }
                else
                {
                    quitRequest = false;
                    textInfo.text = "Quit";

                    //if used mounts extended addon
                    //player.mountsExtended.CmdMountUnSummonThenQuit();

                    NetworkManagerSurvival.Quit();
                }
            }

            if (panel.activeSelf)
            {
                restartButton.interactable = player.remainingLogoutTime == 0;
                quitButton.interactable = player.remainingLogoutTime == 0;

                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(prefab.gameObject, menuList.Count, content);

                // refresh all buttons
                for (int i = 0; i < menuList.Count; ++i)
                {
                    UIButtonSlot slot = content.GetChild(i).GetComponent<UIButtonSlot>();
                    slot.gameObject.name = menuList[i].name;

                    slot.image.sprite = menuList[i].image;
                    slot.textName.text = menuList[i].name;
                    slot.textKey.text = menuList[i].currentKey.ToString();

                    //button
                    int icopy = i; // needed for lambdas, otherwise i is Count
                    slot.button.onClick.SetListener(() =>
                    {
                        if (!restartRequest && !quitRequest)
                        {
                            soundManager.PlaySoundButtonClick();
                            panel.SetActive(false);

                            if (menuList[icopy].panel != null) menuList[icopy].panel.SetActive(!menuList[icopy].panel.activeSelf);

                            for (int x = 0; x < menuList[icopy].openAdditionalPanels.Length; x++)
                            {
                                if (menuList[icopy].openAdditionalPanels[x] != null) menuList[icopy].openAdditionalPanels[x].SetActive(true);
                            }
                        }
                    });
                }
            }
        }

        //Keys
        void LoadKeysFromPlayerPrefs()
        {
            for (int i = 0; i < menuList.Count; ++i)
            {
                if (PlayerPrefs.HasKey(menuList[i].name)) menuList[i].currentKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(menuList[i].name));
                else menuList[i].currentKey = menuList[i].defaultKey;
            }
        }
        public void SaveKeysToPlayerPrefs()
        {
            for (int i = 0; i < menuList.Count; ++i)
                PlayerPrefs.SetString(menuList[i].name, menuList[i].currentKey.ToString()); //save new key to PlayerPrefs
        }
        public void SetKeysDefault()
        {
            for (int i = 0; i < menuList.Count; ++i)
                menuList[i].currentKey = menuList[i].defaultKey;

            SaveKeysToPlayerPrefs();
        }

        //if used Guild Extended addon
        public void guildExtended(Player player)
        {
            //player.guildExtended.CmdLoadAllGuilds();
        }
    }
}


