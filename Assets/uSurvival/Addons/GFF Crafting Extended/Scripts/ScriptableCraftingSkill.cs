using System;
using UnityEngine;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "GFF Addons/Crafting Extended/Crafting Skill", order = 999)]
    public class ScriptableCraftingSkill : ScriptableObject
    {
        public int maxLevel = 100;

        [SerializeField] public ExponentialUInt _experienceMax = new ExponentialUInt { multiplier = 100, baseValue = 1.1f };
    }

    [Serializable]
    public struct ScriptableCraftingSkillAndExp
    {
        public ScriptableCraftingSkill skill;
        public int requiredLevel;
        public uint addsExp;
    }
}


