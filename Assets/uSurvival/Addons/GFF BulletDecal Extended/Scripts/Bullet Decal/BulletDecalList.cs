using System;
using UnityEngine;

namespace GFFAddons
{
    [CreateAssetMenu(fileName = "Bullet Decal List", menuName = "Weapons/Decal/List")]
    public class BulletDecalList : ScriptableObject
    {
        public int genericSurfaceId = 0;
        public SurfaceDecal[] surfaceDecals;

        public SurfaceDecal GetDecalForSurface(string surfaceTag)
        {
            for (int i = 0; i < surfaceDecals.Length; i++)
            {
                if (surfaceDecals[i].SurfaceTag == surfaceTag)
                {
                    return surfaceDecals[i];
                }
            }
            return surfaceDecals[genericSurfaceId];
        }

        public SurfaceDecal GetDecalForTag(Transform trans)
        {
            for (int i = 0; i < surfaceDecals.Length; i++)
            {
                if (trans.CompareTag(surfaceDecals[i].SurfaceTag))
                {
                    return surfaceDecals[i];
                }
            }
            return surfaceDecals[genericSurfaceId];
        }

        [Serializable]
        public class SurfaceDecal
        {
            public string SurfaceTag;
            public Material[] DecalMaterials;
            public Vector2 SizeRange = new Vector2(0.9f, 1.2f);

            public Material GetMaterial()
            {
                return DecalMaterials[UnityEngine.Random.Range(0, DecalMaterials.Length)];
            }
        }
    }
}