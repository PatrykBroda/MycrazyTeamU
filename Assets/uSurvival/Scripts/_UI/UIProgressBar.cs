using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace uSurvival
{
    public class UIProgressBar : MonoBehaviour
    {
        public GameObject panel;
        public Slider slider;
        public TextMeshProUGUI actionText;

        private bool ReloadInProgress(Player player, out float percentage, out string action)
        {
            percentage = 0;
            action = "";

            // currently reloading?
            ItemSlot slot = player.equipment.slots[player.equipment.selection];
            if (slot.amount > 0 && slot.item.data is RangedWeaponItem)
            {
                float reloadTime = ((RangedWeaponItem)slot.item.data).reloadTime;
                if (player.reloading.ReloadTimeRemaining() > 0)
                {
                    percentage = (reloadTime - player.reloading.ReloadTimeRemaining()) / reloadTime;
                    action = Localization.Translate("Reloading");
                    return true;
                }
            }

            return false;
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                float percentage;
                string action;

                //  reloading?
                if (ReloadInProgress(player, out percentage, out action))
                {
                    panel.SetActive(true);
                    slider.value = percentage;
                    actionText.text = action;
                }
                // otherwise hide
                else panel.SetActive(false);
            }
            else panel.SetActive(false);
        }
    }
}