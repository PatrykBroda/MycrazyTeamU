using Mirror;
using UnityEngine;

namespace GFFAddons
{
    public class UpdatedDisableAfter : MonoBehaviour
    {
        public float time = 2.5f;
        double startTime;

        public void Show()
        {
            startTime = NetworkTime.time;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (NetworkTime.time > startTime + time) gameObject.SetActive(false);
        }
    }
}