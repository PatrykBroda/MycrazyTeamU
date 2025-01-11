using UnityEngine;

public class PoolObjectDeactivator : MonoBehaviour
{
    public bool pooled = true;
    public float lifeTime = 5.0f;

    private void OnEnable()
    {
        Invoke(nameof(Disable), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Disable()
    {
        if (pooled)
        {
            transform.parent = ObjectPoolingBase.Instance.transform;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}