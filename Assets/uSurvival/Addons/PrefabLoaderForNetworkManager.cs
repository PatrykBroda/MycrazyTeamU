using Mirror;
using System;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [Serializable]
    public class PrefabsLoaderComponent
    {
        public string name;
        public GameObject[] prefabs;
    }

    public class PrefabLoaderForNetworkManager : MonoBehaviour
    {
        [SerializeField] private NetworkManagerSurvival manager;
        [SerializeField] private PrefabsLoaderComponent[] array;
        public uint assetId;

        public void FillArray()
        {
            manager.spawnPrefabs.Clear();

            for (int i = 0; i < array.Length; i++)
            {
                for (int x = 0; x < array[i].prefabs.Length; x++)
                {
                    if (CheckPrefab(array[i].prefabs[x])) manager.spawnPrefabs.Add(array[i].prefabs[x]);
                }
            }
        }

        public void Clear()
        {
            manager.spawnPrefabs.Clear();
        }

        private bool CheckPrefab(GameObject go)
        {
            for (int i = 0; i < manager.spawnPrefabs.Count; i++)
            {
                if (manager.spawnPrefabs[i] == go) return false;
            }

            return true;
        }

        public void FindByAssetId()
        {
            for (int i = 0; i < manager.spawnPrefabs.Count; i++)
            {
                if (manager.spawnPrefabs[i] != null && manager.spawnPrefabs[i].GetComponent<NetworkIdentity>().assetId == assetId)
                {
                    Debug.Log(manager.spawnPrefabs[i].name);
                }
            }
            Debug.Log("Find end");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            manager = FindObjectOfType<NetworkManagerSurvival>();
        }
#endif
    }
}