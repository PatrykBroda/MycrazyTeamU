using UnityEditor;
using UnityEngine;

namespace GFFAddons
{
    public class CustomButton : MonoBehaviour
    {
        [CustomEditor(typeof(PrefabLoaderForNetworkManager))]
        public class Button : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                PrefabLoaderForNetworkManager myScript = (PrefabLoaderForNetworkManager)target;
                if (GUILayout.Button("Fill Arrays"))
                {
                    myScript.FillArray();
                }

                if (GUILayout.Button("Clear Spawn Prefabs"))
                {
                    myScript.Clear();
                }

                if (GUILayout.Button("Find By Asset id"))
                {
                    myScript.FindByAssetId();
                }
            }
        }
    }
}