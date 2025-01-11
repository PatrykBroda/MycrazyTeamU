using Mirror;
using System;

namespace GFFAddons
{
    [Serializable]public struct CraftingSkill
    {
        public string name;
        public int level;
        public uint exp;
        public int maxLevel;
        public ExponentialUInt _experienceMax;

        // constructors
        public CraftingSkill(ScriptableCraftingSkill data)
        {
            name = data.name;
            level = 0;
            exp = 0;
            maxLevel = data.maxLevel;
            _experienceMax = data._experienceMax;
        }

        public float GetPercent()
        {
            return exp > 0 ? (100 *(float)exp / (float)_experienceMax.Get(level + 1)) : 0;
        }
    }
    public class SyncListCraftingSkill : SyncList<CraftingSkill> { }
}


