using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIExchangeShopCoins : MonoBehaviour
    {
        public GameObject panel;
        public Slider slider;
        public Button buttonExchange;
        public Text textSelectValue;
        public Text textMaxValue;

        public static UIExchangeShopCoins singleton;
        public UIExchangeShopCoins()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            //if (singleton == null)
            singleton = this;
        }

        public void Show() { panel.SetActive(true); }

        void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player)
                {
                    slider.maxValue = player.inventory.Count(player.itemMall.shopItem);
                    textMaxValue.text = slider.maxValue.ToString();
                    textSelectValue.text = slider.value.ToString();

                    buttonExchange.onClick.SetListener(() =>
                    {
                        player.itemMall.CmdExchangeCoinsItem((ushort)slider.value);
                    });
                }
                else panel.SetActive(false);
            }
        }
    }
}


