// Destroys the GameObject after a certain time.
using UnityEngine;

namespace uSurvival
{
    public class DestroyAfter : MonoBehaviour
    {
        public float time = 1;

        void Start()
        {
            Destroy(gameObject, time);
        }
    }
}