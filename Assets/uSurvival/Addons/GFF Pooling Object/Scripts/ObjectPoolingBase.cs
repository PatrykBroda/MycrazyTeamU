using UnityEngine;

public abstract class ObjectPoolingBase : MonoBehaviour
{
    /// <summary>
    /// Get a pooled prefab instance.
    /// </summary>
    public abstract GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation);

    private static ObjectPoolingBase _instance;
    public static ObjectPoolingBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<ObjectPoolingBase>(); }
            return _instance;
        }
    }
}