using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GFFAddons
{
    public class UISoundManager : MonoBehaviour
    {
        public AudioMixer mixer;

        [Header("Settings : Run Sounds")]
        public float SFXmaxDistance = 30f;
        public float SFXRunTime = .7f;

        [Header("UI Elements")]
        public Slider sliderMasterVolume;
        public Slider sliderUIVolume;
        public Slider sliderMusicVolume;
        public Slider sliderSFXVolume;

        [Header("Sounds : interface")]
        public AudioClip buttonClick;
        public AudioClip panelOpen;
        public AudioClip itemSwap;
        public AudioClip itemEquip;
        public AudioClip falseSound;

        [Header("GFF Character Selection")]
        public AudioClip CharacterSelection;
        public AudioClip CharacterSelectionEnd;

        [Header("GFF Character Creation")]
        public AudioClip createCharacterFailed;

        [Header("Skills")]
        public AudioClip SkillUpgrade;

        //singleton
        public static UISoundManager singleton;
        public UISoundManager() { singleton = this; }

        private void Start()
        {
            SetSlidersValues();

            sliderMasterVolume.onValueChanged.AddListener(delegate { SaveMasterVolume(); });
            sliderMusicVolume.onValueChanged.AddListener(delegate { SaveMusicVolume(); });
            sliderSFXVolume.onValueChanged.AddListener(delegate { SaveSFXVolume(); });
            sliderUIVolume.onValueChanged.AddListener(delegate { SaveUIVolume(); });
        }

        public void SetSlidersValues()
        {
            sliderMasterVolume.value = GetMasterVolume();
            sliderMusicVolume.value = GetMusicVolume();
            sliderSFXVolume.value = GetSFXVolume();
            sliderUIVolume.value = GetUIVolume();
        }

        private float GetMasterVolume()
        {
            float value;
            bool result = mixer.GetFloat("MasterVolume", out value);
            if (result) return value;
            else return 0f;
        }
        private float GetMusicVolume()
        {
            float value;
            bool result = mixer.GetFloat("MusicVolume", out value);
            if (result) return value;
            else return 0f;
        }
        private float GetSFXVolume()
        {
            float value;
            bool result = mixer.GetFloat("SFXVolume", out value);
            if (result) return value;
            else return 0f;
        }
        private float GetUIVolume()
        {
            float value;
            bool result = mixer.GetFloat("UIVolume", out value);
            if (result) return value;
            else return 0f;
        }

        private void SaveMasterVolume()
        {
            mixer.SetFloat("MasterVolume", sliderMasterVolume.value);
            PlayerPrefs.SetFloat("VolumeMaster", sliderMasterVolume.value); //save new key to PlayerPrefs
        }
        private void SaveMusicVolume()
        {
            mixer.SetFloat("MusicVolume", sliderMusicVolume.value);
            PlayerPrefs.SetFloat("VolumeMusic", sliderMusicVolume.value); //save new key to PlayerPrefs
        }
        private void SaveSFXVolume()
        {
            mixer.SetFloat("SFXVolume", sliderSFXVolume.value);
            PlayerPrefs.SetFloat("VolumeSFX", sliderSFXVolume.value); //save new key to PlayerPrefs
        }
        private void SaveUIVolume()
        {
            mixer.SetFloat("UIVolume", sliderUIVolume.value);
            PlayerPrefs.SetFloat("VolumeUI", sliderUIVolume.value); //save new key to PlayerPrefs
        }

        public static void PlaySoundUI(AudioClip clip)
        {
            if (clip != null)
            {
                GameObject snd = new GameObject("Sound");
                AudioSource source = snd.AddComponent<AudioSource>();
                //source.volume = volumeSFX;
                source.PlayOneShot(clip);
                Destroy(snd, clip.length);
            }
        }

        public void PlaySound(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                GameObject snd = new GameObject("Sound");
                snd.transform.position = position;
                AudioSource source = snd.AddComponent<AudioSource>();
                //source.volume = volumeSFX;
                source.maxDistance = SFXmaxDistance;
                source.spatialBlend = 1f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.dopplerLevel = 0f;

                source.PlayOneShot(clip);

                Object.Destroy(snd, clip.length);
            }
        }

        //Sound settings
        public void PlaySoundButtonClick()
        {
            PlaySoundUI(buttonClick);
        }
        public void PlaySoundOpenPanel()
        {
            PlaySoundUI(panelOpen);
        }
    }
}


