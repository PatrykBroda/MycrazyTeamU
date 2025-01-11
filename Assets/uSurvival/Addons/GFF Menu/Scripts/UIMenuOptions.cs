using System;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIMenuOptions : MonoBehaviour
    {
        public KeyCode hotKey = KeyCode.Escape;

        public GameObject[] panelCloseByHotKey;

        [Header("Components")]
        public UIMenuHelper menuHelperScript;
        public UISoundManager soundManagerScript;
        public SettingsLoader settings;
        public AudioSource audioSource;

        [Header("Colors")]
        public Color active = Color.yellow;
        public Color disable = Color.grey;

        [Header("UI Elements")]
        public GameObject panelMenu;
        public GameObject panelOptions;

        public Button buttonMisc;
        public Button buttonVideo;
        public Button buttonSound;
        public Button buttonKeys;

        public GameObject panelMisc;
        public GameObject panelVideo;
        public GameObject panelSound;
        public GameObject panelKeys;

        public Button buttonSave;
        public Button buttonDefault;

        [Header("Misc")]
        public Toggle toggleChat;
        public Toggle toggleMiniMap;
        public Toggle toggleBlockParty;
        public Toggle toggleBlockTrade;
        public Toggle toggleBlockPrivateMessages;
        public Toggle toggleBlockGuildInvite;
        public Toggle toggleDamageInfo;
        public Toggle toggleShowMonstersNames;
        public Toggle toggleShowPlayersNames;
        public Toggle toggleShowPlayersGuildName;
        public Toggle toggleShowGuildName;
        public Toggle toggleShowNpcNames;
        public Dropdown dropdownLanguages;

        private void Start()
        {
            //select options type
            buttonMisc.onClick.AddListener(() =>
            {
                audioSource.Play();
                panelMisc.SetActive(true);
                panelSound.SetActive(false);
                panelVideo.SetActive(false);
                panelKeys.SetActive(false);

                //change color 
                buttonMisc.GetComponentInChildren<Text>().color = active;
                buttonSound.GetComponentInChildren<Text>().color = disable;
                buttonVideo.GetComponentInChildren<Text>().color = disable;
                buttonKeys.GetComponentInChildren<Text>().color = disable;
            });
            buttonVideo.onClick.AddListener(() =>
            {
                audioSource.Play();
                panelMisc.SetActive(false);
                panelSound.SetActive(false);
                panelVideo.SetActive(true);
                panelKeys.SetActive(false);

                //change color 
                buttonMisc.GetComponentInChildren<Text>().color = disable;
                buttonSound.GetComponentInChildren<Text>().color = disable;
                buttonVideo.GetComponentInChildren<Text>().color = active;
                buttonKeys.GetComponentInChildren<Text>().color = disable;
            });
            buttonSound.onClick.AddListener(() =>
            {
                audioSource.Play();
                panelMisc.SetActive(false);
                panelSound.SetActive(true);
                panelVideo.SetActive(false);
                panelKeys.SetActive(false);

                //change color 
                buttonMisc.GetComponentInChildren<Text>().color = disable;
                buttonSound.GetComponentInChildren<Text>().color = active;
                buttonVideo.GetComponentInChildren<Text>().color = disable;
                buttonKeys.GetComponentInChildren<Text>().color = disable;
            });
            buttonKeys.onClick.AddListener(() =>
            {
                audioSource.Play();
                panelMisc.SetActive(false);
                panelSound.SetActive(false);
                panelVideo.SetActive(false);
                panelKeys.SetActive(true);

                //change color 
                buttonMisc.GetComponentInChildren<Text>().color = disable;
                buttonSound.GetComponentInChildren<Text>().color = disable;
                buttonVideo.GetComponentInChildren<Text>().color = disable;
                buttonKeys.GetComponentInChildren<Text>().color = active;
            });

            buttonDefault.onClick.SetListener(() => {
                audioSource.Play();

                if (panelKeys.activeSelf)
                {
                    menuHelperScript.SetKeysDefault();
                }
                else if (panelMisc.activeSelf)
                {
                    toggleChat.isOn = true;
                    toggleMiniMap.isOn = true;
                    toggleBlockParty.isOn = false;
                    toggleBlockTrade.isOn = false;
                    toggleBlockPrivateMessages.isOn = false;
                    toggleBlockGuildInvite.isOn = false;
                    toggleDamageInfo.isOn = true;

                    toggleShowMonstersNames.isOn = true;
                    toggleShowPlayersNames.isOn = true;
                    toggleShowGuildName.isOn = true;
                    toggleShowPlayersGuildName.isOn = true;
                    toggleShowNpcNames.isOn = true;

                    settings.SetDefaultValues();
                }
            });

            toggleChat.isOn = SettingsLoader._chat;
            toggleMiniMap.isOn = SettingsLoader._miniMap;
            toggleBlockParty.isOn = SettingsLoader._blockParty;
            toggleBlockTrade.isOn = SettingsLoader._blockTrade;
            toggleBlockPrivateMessages.isOn = SettingsLoader._blockPrivateMessages;
            toggleBlockGuildInvite.isOn = SettingsLoader._blockGuildInvite;
            toggleDamageInfo.isOn = SettingsLoader._damageInfo;

            toggleShowMonstersNames.isOn = SettingsLoader._showMonstersNames;
            toggleShowPlayersNames.isOn = SettingsLoader._showPlayersNames;
            toggleShowPlayersGuildName.isOn = SettingsLoader._showPlayersGuildName;
            toggleShowGuildName.isOn = SettingsLoader._showGuildName;
            toggleShowNpcNames.isOn = SettingsLoader._showNpcNames;

            //Save Misc settings
            toggleChat.onValueChanged.AddListener(delegate { SettingsLoader._chat = toggleChat.isOn; PlayerPrefs.SetInt("Chat", toggleChat.isOn ? 1 : 0); });
            toggleMiniMap.onValueChanged.AddListener(delegate { SettingsLoader._miniMap = toggleMiniMap.isOn; PlayerPrefs.SetInt("MiniMap", toggleMiniMap.isOn ? 1 : 0); });
            toggleBlockParty.onValueChanged.AddListener(delegate { PlayerPrefs.SetInt("BlockParty", toggleBlockParty.isOn ? 1 : 0); });
            toggleBlockTrade.onValueChanged.AddListener(delegate { PlayerPrefs.SetInt("BlockTrade", toggleBlockTrade.isOn ? 1 : 0); });
            toggleBlockPrivateMessages.onValueChanged.AddListener(delegate { PlayerPrefs.SetInt("BlockPrivateMessages", toggleBlockPrivateMessages.isOn ? 1 : 0); });
            toggleBlockGuildInvite.onValueChanged.AddListener(delegate { PlayerPrefs.SetInt("BlockGuildInvite", toggleBlockGuildInvite.isOn ? 1 : 0); });
            toggleDamageInfo.onValueChanged.AddListener(delegate { SettingsLoader._damageInfo = toggleDamageInfo.isOn; PlayerPrefs.SetInt("DamageInfo", toggleDamageInfo.isOn ? 1 : 0); });

            toggleShowMonstersNames.onValueChanged.AddListener(delegate { SettingsLoader._showMonstersNames = toggleShowMonstersNames.isOn; PlayerPrefs.SetInt("ShowMonstersNames", toggleShowMonstersNames.isOn ? 1 : 0); });
            toggleShowPlayersNames.onValueChanged.AddListener(delegate { SettingsLoader._showPlayersNames = toggleShowPlayersNames.isOn; PlayerPrefs.SetInt("ShowPlayersNames", toggleShowPlayersNames.isOn ? 1 : 0); });
            toggleShowPlayersGuildName.onValueChanged.AddListener(delegate { SettingsLoader._showPlayersGuildName = toggleShowPlayersGuildName.isOn; PlayerPrefs.SetInt("ShowPlayersGuildName", toggleShowPlayersGuildName.isOn ? 1 : 0); });
            toggleShowGuildName.onValueChanged.AddListener(delegate { SettingsLoader._showGuildName = toggleShowGuildName.isOn; PlayerPrefs.SetInt("ShowGuildName", toggleShowGuildName.isOn ? 1 : 0); });
            toggleShowNpcNames.onValueChanged.AddListener(delegate { SettingsLoader._showNpcNames = toggleShowNpcNames.isOn; PlayerPrefs.SetInt("ShowNpcNames", toggleShowNpcNames.isOn ? 1 : 0); });

            dropdownLanguages.AddOptions(Localization.languagesFromCsv);
            dropdownLanguages.value = Localization.languagesFromCsv.FindIndex(s => s == Localization.languageCurrent.ToString());
            dropdownLanguages.onValueChanged.AddListener(delegate
            {
                int index = (int)Enum.Parse(typeof(SystemLanguage), dropdownLanguages.captionText.text);
                Localization.Language = (SystemLanguage)index;
            });
        }

        private void Update()
        {
            //Escape key will open or close the panels
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    bool check = false;
                    for (int i = 0; i < panelCloseByHotKey.Length; i++)
                    {
                        if (panelCloseByHotKey[i] != null && panelCloseByHotKey[i].activeSelf)
                        {
                            panelCloseByHotKey[i].SetActive(false);
                            //player.interactionExtended.CmdSetTargetNull();
                            check = true;
                        }
                    }

                    if (check == false) panelOptions.SetActive(!panelOptions.activeSelf);
                }
            }

            if (panelOptions.activeSelf)
            {
                buttonSave.gameObject.SetActive(!panelSound.activeSelf && !panelMisc.activeSelf);
                buttonDefault.gameObject.SetActive(!panelSound.activeSelf);
            }
        }
    }
}


