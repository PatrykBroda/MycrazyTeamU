// can be added to weapons to define more details like muzzle location, etc.
using UnityEngine;

namespace uSurvival
{
    public class WeaponDetails : MonoBehaviour
    {
        [Header("Muzzle")]
        public MuzzleFlash muzzleFlash;
        public Transform muzzleLocation;

        //GFF
        public AudioSource audioShotSource;
        public AudioSource audioReloadSource;

        public Transform scopeB;
    }
}