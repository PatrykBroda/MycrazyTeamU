using Mirror;
using System;
using UnityEngine;

namespace GFFAddons
{
    [Serializable]
    public struct CombatSkill
    {
        // hashcode used to reference the real ItemTemplate (can't link to template
        // directly because synclist only supports simple types). and syncing a
        // string's hashcode instead of the string takes WAY less bandwidth.
        //public int hash;

        // dynamic stats
        public byte level;
        public long exp;
        //public int maxLevel;
        public ExponentialLong _experienceMax;

        /* [Header("Health")]
         public int bonusHealth;
         [Range(0, 1)] public float bonusHealthPercent;

         [Header("Mana")]
         public int bonusMana;
         [Range(0, 1)] public float bonusManaPercent;

         [Header("if use Stamina addon")]
         public int bonusStamina;
         [Range(0, 1)] public float bonusStaminaPercent;

         [Header("Damage / Defense")]
         public float bonusDamage;
         public float bonusDefense;

         [Header("Block Chance")]
         [Range(0, 1)] public float bonusBlockChance;*/

        // constructors
        public CombatSkill(CombatSkillItem data)
        {
            //hash = data.name.GetStableHashCode();

            level = 0;
            exp = 0;
            //maxLevel = data.maxLevel;
            _experienceMax = data._experienceMax;
        }

        public float GetPercent()
        {
            return (exp != 0 && _experienceMax.Get(level) != 0) ? (float)exp / (float)_experienceMax.Get(level) : 0;
        }
    }

    public class SyncListCombatSkill : SyncList<CombatSkill> { }
}