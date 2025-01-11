using TMPro;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public class UIInventoryGold : MonoBehaviour
    {
        public TextMeshProUGUI textGoldValue;

        // Update is called once per frame
        void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                textGoldValue.text = player.gold == 0 ? "0" : player.gold.ToString("###,###,###,###");
            }
        }
    }
}