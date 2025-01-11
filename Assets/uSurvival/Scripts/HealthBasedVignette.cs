using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace uSurvival
{
    public class HealthBasedVignette : MonoBehaviour
    {
        public PostProcessProfile volumeProfile;
        private Vignette vignette;

        public Color colorNormal = Color.black;
        public float intensityNormal = 0.3f;

        public Color colorDamaged = Color.red;
        public float healthBasedSpeedMultiplier = 1;

        void Start()
        {
            // get the vignette effect
            for (int i = 0; i < volumeProfile.settings.Count; i++)
            {
                if (volumeProfile.settings[i].name == "Vignette")
                {
                    vignette = (Vignette)volumeProfile.settings[i];
                }
            }

            SetVignetteSmoothness(intensityNormal, colorNormal);
        }

        void Update()
        {
            Player player = Player.localPlayer;
            if (!player) return;

            float healthPercent = player.health.Percent();
            if (healthPercent < 1)
            {
                float speed = 1 + (1 - healthPercent) * healthBasedSpeedMultiplier; // scale speed with health
                float wave = Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * speed));
                SetVignetteSmoothness((1 - healthPercent) * (0.5f + (wave / 2f)), colorDamaged);
            }
            else
            {
                SetVignetteSmoothness(intensityNormal, colorNormal);
            }
        }

        private void SetVignetteSmoothness(float value, Color color)
        {
            vignette.intensity.Override(value);
            vignette.color.Override(color);
        }
    }
}