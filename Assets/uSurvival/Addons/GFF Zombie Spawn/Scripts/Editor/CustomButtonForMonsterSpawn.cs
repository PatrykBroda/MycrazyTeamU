using GFFAddons;
using UnityEditor;
using UnityEngine;

public class CustomButtonForMonsterSpawn : MonoBehaviour
{
    [CustomEditor(typeof(ZombieSpawnSystem))]
    public class Button : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ZombieSpawnSystem myScript = (ZombieSpawnSystem)target;
            if (GUILayout.Button("Add Child Gameobjects To SpawnPoints"))
            {
                myScript.AddChildGameobjectsToSpawnPoints();
            }
        }
    }
}
