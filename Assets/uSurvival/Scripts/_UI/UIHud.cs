using GFFAddons;
using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace uSurvival
{
    public class UIHud : MonoBehaviour
    {
        public KeyCode hotKey = KeyCode.LeftAlt;

        [Header("Colors")]
        public Color brokenDurabilityColor = Color.red;
        public Color lowDurabilityColor = Color.magenta;
        [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

        [Header("Elements")]
        public GameObject panel;

        public Slider sliderBoost;

        //Health
        public Image imageHealth;
        public TextMeshProUGUI textHealth;

        //Hydration
        public Image imageHydration;
        public TextMeshProUGUI textHydration;

        //Nutrition
        public Image imageNutrition;
        public TextMeshProUGUI textNutrition;

        //Endurance
        public Image imageEndurance;
        public TextMeshProUGUI textEndurance;

        //Weight
        public Image imageWeight;
        public GameObject weightOverload;

        [Header("Elements Disabled In Game")]
        public GameObject panelMainMenu;
        public GameObject elementsDisabledInGame;

        [Header("Lifetime, PlayersKilled, MonstersKilled")]
        public TextMeshProUGUI textLifetime;
        public TextMeshProUGUI textPlayersKilled;
        public TextMeshProUGUI textMonstersKilled;

        [Header("Damage, Defense, MoveSpeed")]
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI defenseText;
        public TextMeshProUGUI moveSpeedText;

        [Header("Respawn")]
        public GameObject panelRespawn;
        public Text timeText;

        private IEnumerator Start()
        {
            while (true)
            {
                Player player = Player.localPlayer;
                if (player)
                {
                    panel.SetActive(true);
                    UpdateHealthValue(player.health);
                    UpdateHydrationValue(player.hydration);
                    UpdateNutritionValue(player.nutrition);
                    UpdateEnduranceValue(player.endurance);

                    //weight
                    float weightPercent = player.weight.Percent();
                    imageWeight.fillAmount = weightPercent;

                    if (imageWeight.fillAmount == 1)
                        imageWeight.color = brokenDurabilityColor;
                    else if (imageWeight.fillAmount > lowDurabilityThreshold)
                        imageWeight.color = lowDurabilityColor;
                    else imageWeight.color = Color.white;

                    weightOverload.SetActive(weightPercent >= 1);

                    //boost
                    sliderBoost.value = player.boosts.boost;

                    if (elementsDisabledInGame.activeSelf)
                    {
                        damageText.text = player.combat.damage.ToString();
                        defenseText.text = player.combat.defense.ToString();
                        moveSpeedText.text = player.movement.moveSpeed.ToString("F2");

                        textPlayersKilled.text = player.statistics.playersKilled.ToString();
                        textMonstersKilled.text = player.statistics.monstersKilled.ToString();
                        textLifetime.text = UtilsExtended.PrettySeconds((float)player.statistics.lifetime);
                    }

                    if (panelRespawn.activeSelf)
                    {
                        // calculate the respawn time remaining for the client
                        double remaining = player.respawning.respawnTimeEnd - NetworkTime.time;
                        timeText.text = remaining.ToString("F0");
                    }
                }
                else
                {
                    panel.SetActive(false);
                    panelRespawn.SetActive(false);
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                elementsDisabledInGame.SetActive(Input.GetKey(hotKey) && !UIUtils.AnyInputActive());

                //Respawn panel 
                panelRespawn.SetActive(player.health.current <= 0);

                // hotkey (not while typing in chat, etc.)
                //if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                //{

                //}
            }
        }

        private void UpdateHealthValue(Health health)
        {
            imageHealth.fillAmount = (health.current != 0 && health.max != 0) ? (float)health.current / health.max : 0;
            textHealth.text = health.current + " / " + health.max;
        }
        private void UpdateHydrationValue(Hydration hydration)
        {
            imageHydration.fillAmount = (hydration.current != 0 && hydration.max != 0) ? (float)hydration.current / hydration.max : 0;
            textHydration.text = hydration.current + " / " + hydration.max;
        }
        private void UpdateNutritionValue(Nutrition nutrition)
        {
            imageNutrition.fillAmount = (nutrition.current != 0 && nutrition.max != 0) ? (float)nutrition.current / nutrition.max : 0;
            textNutrition.text = nutrition.current + " / " + nutrition.max;
        }
        private void UpdateEnduranceValue(Endurance endurance)
        {
            imageEndurance.fillAmount = (endurance.current != 0 && endurance.max != 0) ? (float)endurance.current / endurance.max : 0;
            textEndurance.text = endurance.current + " / " + endurance.max;
        }
    }
}