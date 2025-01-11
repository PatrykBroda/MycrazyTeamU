using UnityEngine;

namespace GFFAddons
{
    public class RadiationZone : MonoBehaviour
    {
        [SerializeField] private int _decreaseHealth = -10;
        [SerializeField] private int _increaseRadiation = 5;

        public int decreaseHealth  => _decreaseHealth;
        public int increaseRadiation  => _increaseRadiation;

        [SerializeField] private Color gizmoColor = new Color(0, 1, 1, 0.25f);
        [SerializeField] private Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

        // draw the zone all the time, otherwise it's not too obvious where the zones are
        private void OnDrawGizmos()
        {
            SphereCollider collider = GetComponent<SphereCollider>();

            // we need to set the gizmo matrix for proper scale & rotation
            //Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(collider.transform.position, collider.radius);
            Gizmos.color = gizmoWireColor;
            Gizmos.DrawWireSphere(collider.transform.position, collider.radius);
        }
    }
}