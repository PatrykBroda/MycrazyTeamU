using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make sure you served the default prefabs from the default pool system with the same key/identifiers.
/// </summary>
public class ObjectPooling : ObjectPoolingBase
{
    public bool isEnable = true;
    public class PoolObject
    {
        public string name;
        public GameObject prefab;
        public GameObject[] poolList;
        public int currentPool;

        public GameObject GetCurrent() => poolList[currentPool];
        public void SetNext() { currentPool = (currentPool + 1) % poolList.Length; }
        public void ReplaceCurrent(GameObject g) => poolList[currentPool] = g;
    }

    [Serializable]public class PreRegister
    {
        public string name;
        public GameObject Prefab;
        public int lenght;
    }

    [SerializeField] private PreRegister[] pooledPrefabs;

    private Dictionary<string, PoolObject> pools = new Dictionary<string, PoolObject>();

    private void Awake()
    {
        if (isEnable)
        {
            for (int i = 0; i < pooledPrefabs.Length; i++)
            {
                RegisterObject(pooledPrefabs[i].name, pooledPrefabs[i].Prefab, pooledPrefabs[i].lenght);
            }
        }
    }

    /// <summary>
    /// Add a new pooled prefab
    /// </summary>
    public void RegisterObject(string poolName, GameObject prefab, int count)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Can't pooled the prefab for: " + poolName + " because the prefab has not been assigned.");
            return;
        }

        var p = new PoolObject();
        p.name = poolName;
        p.prefab = prefab;
        GameObject g;
        p.poolList = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            g = Instantiate(prefab, transform) as GameObject;
            p.poolList[i] = g;
            g.SetActive(false);
        }
        pools.Add(poolName, p);
    }

    /// <summary>
    /// Instantiate a pooled prefab
    /// use this instead of GameObject.Instantiate(...)
    /// </summary>
    public override GameObject Instantiate(string objectName, Vector3 position, Quaternion rotation)
    {
        PoolObject pool = pools[objectName];
        if (pool != null)
        {
            GameObject g = pool.GetCurrent();
            if (g == null)//in case a pool object get destroyed, replace it 
            {
                g = Instantiate(pool.prefab, transform);
                pool.ReplaceCurrent(g);
            }
            g.transform.SetPositionAndRotation(position, rotation);
            g.SetActive(true);
            pool.SetNext();
            return g;
        }
        else
        {
            Debug.LogError(string.Format("Object {0} has not been register for pooling.", objectName));
            return null;
        }
    }
}