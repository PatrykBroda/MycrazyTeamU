using UnityEngine;
using UnityEngine.UI;

namespace GFFAddons
{
    public class UIAim : MonoBehaviour
    {
        [SerializeField] private Image imageScope;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject[] windowsThatLockPanel;
        [SerializeField] private GameObject panelCrosshair;

        //singleton
        public static UIAim singleton;
        public UIAim()
        {
            singleton = this;
        }

        public void Show(Sprite sprite)
        {
            if (!AnyLockWindowActive())
            {
                imageScope.sprite = sprite;
                panel.SetActive(true);
            }
        }

        public void Hide() { panel.SetActive(false); }

        private void Update()
        {
            panelCrosshair.SetActive(panel.activeSelf == false);

            //Player player = Player.localPlayer;
            //if (player)
            //{
            //    if (Input.GetMouseButton(1) && player.reloading.ReloadTimeRemaining() == 0)
            //    {

            //    }
            //    else{Hide();}
            //}
            //else { Hide(); }
        }

        public bool AnyLockWindowActive()
        {
            // check manually. Linq.Any() is HEAVY(!) on GC and performance
            foreach (GameObject go in windowsThatLockPanel)
                if (go.activeSelf)
                    return true;
            return false;
        }
    }
}