using UnityEngine;
using UnityEngine.Audio;

namespace GFFAddons
{
    public class SettingsLoader : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private bool updateDebugger = false;

        public static bool _chat = true;
        public static bool _miniMap = true;
        public static bool _blockParty;
        public static bool _blockTrade;
        public static bool _blockPrivateMessages;
        public static bool _blockGuildInvite;
        public static bool _damageInfo;

        public static bool _showMonstersNames = true;
        public static bool _showPlayersNames = true;
        public static bool _showPlayersGuildName = true;
        public static bool _showPlayersKlanName = true;
        public static bool _showGuildName = false;
        public static bool _showKlanName = true;
        public static bool _showNpcNames = true;

        private void OnEnable()
        {
            if (updateDebugger)
            {
#if UNITY_EDITOR
                Debug.unityLogger.logEnabled = true;
#elif UNITY_STANDALONE_WIN
             Debug.unityLogger.logEnabled = false;
#else
             Debug.unityLogger.logEnabled = true;
#endif
            }
        }

        private void Start()
        {
            LoadAudioSettings();
            LoadGameSettins();
        }

        private void LoadAudioSettings()
        {
            if (mixer)
            {
                if (PlayerPrefs.HasKey("VolumeMaster"))
                {
                    mixer.SetFloat("MasterVolume", PlayerPrefs.GetFloat("VolumeMaster"));
                }
                if (PlayerPrefs.HasKey("VolumeMusic"))
                {
                    mixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("VolumeMusic"));
                }
                if (PlayerPrefs.HasKey("VolumeSFX"))
                {
                    mixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("VolumeSFX"));
                }
                if (PlayerPrefs.HasKey("VolumeUI"))
                {
                    mixer.SetFloat("UIVolume", PlayerPrefs.GetFloat("VolumeUI"));
                }
            }
        }

        private void LoadGameSettins()
        {
            //Misc settings
            if (PlayerPrefs.HasKey("Chat")) _chat = PlayerPrefs.GetInt("Chat") == 1 ? true : false;
            if (PlayerPrefs.HasKey("MiniMap")) _miniMap = PlayerPrefs.GetInt("MiniMap") == 1 ? true : false;
            if (PlayerPrefs.HasKey("BlockParty")) _blockParty = PlayerPrefs.GetInt("BlockParty") == 1 ? true : false;
            if (PlayerPrefs.HasKey("BlockTrade")) _blockTrade = PlayerPrefs.GetInt("BlockTrade") == 1 ? true : false;
            if (PlayerPrefs.HasKey("BlockPrivateMessages")) _blockPrivateMessages = PlayerPrefs.GetInt("BlockPrivateMessages") == 1 ? true : false;
            if (PlayerPrefs.HasKey("BlockGuildInvite")) _blockGuildInvite = PlayerPrefs.GetInt("BlockGuildInvite") == 1 ? true : false;
            if (PlayerPrefs.HasKey("DamageInfo")) _damageInfo = PlayerPrefs.GetInt("DamageInfo") == 1 ? true : false;

            if (PlayerPrefs.HasKey("ShowMonstersNames")) _showMonstersNames = PlayerPrefs.GetInt("ShowMonstersNames") == 1 ? true : false;
            if (PlayerPrefs.HasKey("ShowPlayersNames")) _showPlayersNames = PlayerPrefs.GetInt("ShowPlayersNames") == 1 ? true : false;
            if (PlayerPrefs.HasKey("ShowPlayersGuildName")) _showPlayersGuildName = PlayerPrefs.GetInt("ShowPlayersGuildName") == 1 ? true : false;
            if (PlayerPrefs.HasKey("ShowGuildName")) _showGuildName = PlayerPrefs.GetInt("ShowGuildName") == 1 ? true : false;
            if (PlayerPrefs.HasKey("ShowNpcNames")) _showNpcNames = PlayerPrefs.GetInt("ShowNpcNames") == 1 ? true : false;
        }

        public void SetDefaultValues()
        {
            _chat = true;
            _miniMap = true;
            _blockParty = false;
            _blockTrade = false;
            _blockPrivateMessages = false;
            _blockGuildInvite = false;
            _damageInfo = false;

            _showMonstersNames = true;
            _showPlayersNames = true;
            _showPlayersGuildName = true;
            _showPlayersKlanName = true;
            _showGuildName = true;
            _showKlanName = true;
            _showNpcNames = true;

            SaveGameSettings();
        }

        private void SaveGameSettings()
        {
            PlayerPrefs.SetInt("Chat", 1);
            PlayerPrefs.SetInt("MiniMap", 1);
            PlayerPrefs.SetInt("BlockParty", 0);
            PlayerPrefs.SetInt("BlockTrade", 0);
            PlayerPrefs.SetInt("BlockPrivateMessages", 0);
            PlayerPrefs.SetInt("BlockGuildInvite", 0);
            PlayerPrefs.SetInt("DamageInfo", 1);

            PlayerPrefs.SetInt("ShowMonstersNames", 1);
            PlayerPrefs.SetInt("ShowPlayersNames", 1);
            PlayerPrefs.SetInt("ShowPlayersGuildName", 1);
            PlayerPrefs.SetInt("ShowGuildName", 1);
            PlayerPrefs.SetInt("ShowNpcNames", 1);
        }
    }
}