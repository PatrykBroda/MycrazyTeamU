using UnityEngine;

namespace GFFAddons
{
    public class GlobalUpdate : MonoBehaviour
    {
        private void Update()
        {
            for (int i = 0; i < MonoCache.allUpdate.Count; i++)
            {
                MonoCache.allUpdate[i].Tick();
            }
        }
    }
}


