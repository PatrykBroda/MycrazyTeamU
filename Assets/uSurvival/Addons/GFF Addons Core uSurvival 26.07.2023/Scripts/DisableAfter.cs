using UnityEngine;

namespace GFFAddons
{
    public class DisableAfter : MonoBehaviour
    {
        public float time = 2.5f;

        private void OnEnable()
        {
            Invoke("Disable", time);
        }

        void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}