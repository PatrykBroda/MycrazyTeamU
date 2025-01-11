using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UILoginExtendedQuitAndRestart : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] buttonsQuit;
        [SerializeField] private Button buttonQuitConfirm;

        [Space (10)]
        [SerializeField] private GameObject panelQuitFromGame;
        [SerializeField] private Button buttonConfirmQuitFromGame;
        [SerializeField] private Button buttonRestart;
        [SerializeField] private Text textInfo;

        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton=null in Start/Awake

        //singleton
        public static UILoginExtendedQuitAndRestart singleton;
        public UILoginExtendedQuitAndRestart() { singleton = this; }

        public void ShowPanelQuitFromGame() { panelQuitFromGame.SetActive(true); }

        private void Start()
        {
            //from Login Extended
            for (int i = 0; i < buttonsQuit.Length; i++)
            {
                int icopy = i;
                buttonsQuit[i].onClick.SetListener(() =>
                {
                    panel.SetActive(true);
                });
            }

            buttonQuitConfirm.onClick.SetListener(() =>
            {
                NetworkManagerSurvival.Quit();
            });

            //from game
            //buttonConfirmQuitFromGame.onClick.SetListener(() =>
            //{
            //    NetworkManagerSurvival.Quit();
            //});

            //buttonRestart.onClick.SetListener(() =>
            //{
            //    StartCoroutine(Restart());
            //});
        }

        private IEnumerator Restart()
        {
            int time = 5;

            while (time > 0)
            {
                textInfo.text = "Restart in " + time + " seconds";
                yield return new WaitForSeconds(1);
                time--;
            }
            if (time == 0)
            {
                panelQuitFromGame.SetActive(false);
                textInfo.text = "What do you want to do?";

                manager.Restart();
            }
        }
    }
}