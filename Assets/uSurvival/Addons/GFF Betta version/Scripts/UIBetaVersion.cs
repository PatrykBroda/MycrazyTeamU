using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIBetaVersion : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button button;
        [SerializeField] private Toggle toggle;

        [SerializeField] private GameObject penelFirstEnter;

        [Header("Components")]
        public AudioSource audioSource;

        private void Start()
        {
            button.onClick.SetListener(() =>
            {
                if (audioSource)audioSource.Play();
                panel.SetActive(false);

                if (PlayerPrefs.HasKey("FirstEnter") == false)
                {
                    penelFirstEnter.SetActive(true);
                    PlayerPrefs.SetInt("FirstEnter", 1);
                }
            });
        }

        public void Show() { panel.SetActive(true); }
    }
}