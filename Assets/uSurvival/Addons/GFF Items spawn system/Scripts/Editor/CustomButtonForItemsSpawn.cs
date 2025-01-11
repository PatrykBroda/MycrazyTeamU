using GFFAddons;
using UnityEditor;
using UnityEngine;

public class CustomButtonForItemsSpawn : MonoBehaviour
{
    [CustomEditor(typeof(ItemsRandomSpawnSystem))]
    public class Button : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ItemsRandomSpawnSystem myScript = (ItemsRandomSpawnSystem)target;
            if (GUILayout.Button("Fill Arrays"))
            {
                myScript.FillArray();
            }

            if (GUILayout.Button("Clear Spawn Prefabs"))
            {
                myScript.Clear();
            }
        }
    }
}
