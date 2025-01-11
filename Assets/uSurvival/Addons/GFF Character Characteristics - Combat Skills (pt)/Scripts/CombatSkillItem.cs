using UnityEngine;

namespace GFFAddons
{
    public enum IncreaseExpType : byte { None, GetHit, GetBlock, MeleeDamage, RangeDamage, ForceDamage, useMedicine, useHands };

    [CreateAssetMenu(menuName = "GFF Addons/Combat Skill", order = 999)]
    public class CombatSkillItem : ScriptableObject
    {
        [Header("way to gain experience")]
        public IncreaseExpType increaseExp;
        public int maxLevel = 100;
        [SerializeField] public ExponentialLong _experienceMax = new ExponentialLong { multiplier = 100, baseValue = 1.1f };

        [Header("Health")]
        public short bonusHealth;
        [Range(0, 1)] public float bonusHealthPercent;

        [Header("Hydration")]
        public int bonusHydration;
        [Range(0, 1)] public float bonusHydrationPercent;

        [Header("Nutrition")]
        public int bonusNutrition;
        [Range(0, 1)] public float bonusNutritionPercent;

        [Header("Endurance")]
        public int bonusEndurance;
        [Range(0, 1)] public float bonusEndurancePercent;

        [Header("Damage")]
        [Range(0, 1)] public float bonusDamageMeleePercent;
        [Range(0, 1)] public float bonusDamageRangePercent;

        [Header("Defense")]
        [Range(0, 1)] public float bonusDefensePercent;

        [Header("Weight")]
        public int bonusWeight;
        [Range(0, 1)] public float bonusWeightPercent;
    }
}