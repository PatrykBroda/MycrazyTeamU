using UnityEngine;

namespace uSurvival
{
    public partial class RangedWeaponItem
    {
        public void InstanceHitDecalAndParticle(RaycastHit hit)
        {
            switch (hit.transform.tag) // decide what the bullet collided with and what to do with it
            {
                case "IgnoreBullet":
                    break;
                case "Projectile":
                    // do nothing if 2 bullets collide
                    break;
                case "Wood":
                    InstanceHitParticle("decalw", hit);
                    break;
                case "Concrete":
                    InstanceHitParticle("decalc", hit);
                    break;
                case "Metal":
                    InstanceHitParticle("decalm", hit);
                    break;
                case "Dirt":
                    InstanceHitParticle("decals", hit);
                    break;
                case "Water":
                    InstanceHitParticle("decalwt", hit);
                    break;
                default:
                    InstanceHitParticle("decal", hit);
                    break;
            }
        }

        private void InstanceHitParticle(string poolPrefab, RaycastHit hit)
        {
            // instantiate particle
            GameObject go = ObjectPoolingBase.Instance.Instantiate(poolPrefab, hit.point, Quaternion.LookRotation(hit.normal)); 

            // instance decal
            BulletDecalManagerBase.InstantiateDecal(hit);

            if (go != null)
                go.transform.parent = hit.transform;
        }
    }
}