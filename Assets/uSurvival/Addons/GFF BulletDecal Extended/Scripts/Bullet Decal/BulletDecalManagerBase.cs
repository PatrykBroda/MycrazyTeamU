using UnityEngine;

public abstract class BulletDecalManagerBase : MonoBehaviour
{
    public static void InstantiateDecal(RaycastHit raycastHit)
    {
        if (Instance == null) return;

        Instance.InstanceDecal(raycastHit);
    }

    public abstract void InstanceDecal(RaycastHit raycastHit);

    private static BulletDecalManagerBase _instance;
    public static BulletDecalManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<BulletDecalManagerBase>(); }
            return _instance;
        }
    }
}