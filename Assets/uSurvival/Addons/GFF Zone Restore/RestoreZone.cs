using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RestoreZone : MonoBehaviour
{
    public short restoreHealth = 10;

    public Color gizmoColor = new Color(0, 1, 1, 0.25f);
    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // draw the zone all the time, otherwise it's not too obvious where the
    // zones are
    void OnDrawGizmos()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        // we need to set the gizmo matrix for proper scale & rotation
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(collider.transform.position, collider.radius);
        Gizmos.color = gizmoWireColor;
        Gizmos.DrawWireSphere(collider.transform.position, collider.radius);
    }
}
