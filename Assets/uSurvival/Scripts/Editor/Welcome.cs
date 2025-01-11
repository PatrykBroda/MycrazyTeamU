// Shows either a welcome message, or warning about a recommended Unity version.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace uSurvival
{
    static class Welcome
    {
        [InitializeOnLoadMethod]
        static void OnInitializeOnLoad()
        {
            // InitializeOnLoad is called on start and after each rebuild,
            // but we only want to show this once per editor session.
            if (!SessionState.GetBool("USURVIVAL_WELCOME", false))
            {
                SessionState.SetBool("USURVIVAL_WELCOME", true);

#if UNITY_2021_3_OR_NEWER
                Debug.Log("uSurvival | u3d.as/TsE | https://discord.gg/2gNKN78");
#else
                Debug.LogWarning("uSurvival works best with Unity 2021.3 LTS!");
#endif
            }
        }
    }
}
#endif