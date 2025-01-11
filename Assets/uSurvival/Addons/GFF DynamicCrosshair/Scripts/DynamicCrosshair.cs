using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public class DynamicCrosshair : MonoCache
    {
        [SerializeField] private RectTransform reticle;

        [SerializeField] private float restingSize;
        [SerializeField] private float maxSize;
        [SerializeField] private float speed;
        private float currentSize;

        [SerializeField] private GameObject panel;

        public override void OnTick()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                panel.SetActive(true);
                if (player.movement.state != MoveState.DEAD && player.movement.state != MoveState.IDLE)
                    currentSize = Mathf.Lerp(currentSize, maxSize, Time.deltaTime * speed);
                else currentSize = Mathf.Lerp(currentSize, restingSize, Time.deltaTime * speed);

                reticle.sizeDelta = new Vector2(currentSize, currentSize);
            }
            else panel.SetActive(false);
        }
    }
}