// put this next to a collider to have multiplied damage, e.g. on the head
using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public enum DamageAreaType {none, Head, Upper, Lower, Hands, Legs}

    [RequireComponent(typeof(Collider))]
    public class DamageArea : MonoBehaviour
    {
        public DamageAreaType damageTypeArea;
        public int[] checkingEquipmentSlots;
        public float multiplier = 1;
    }
}