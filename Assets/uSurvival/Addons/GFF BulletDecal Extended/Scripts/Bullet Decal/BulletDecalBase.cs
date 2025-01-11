using UnityEngine;

namespace GFFAddons
{
    public abstract class BulletDecalBase : MonoBehaviour
    {
        public abstract BulletDecalBase Init(Transform parent);

        public abstract BulletDecalBase SetDecalMaterial(Material mat);

        public abstract BulletDecalBase SetToHit(RaycastHit hit, bool asPendingParent = false);

        public abstract BulletDecalBase SetScaleVariation(Vector3 scaleBase, Vector2 Range);

        public abstract void BackToOrigin();
    }
}