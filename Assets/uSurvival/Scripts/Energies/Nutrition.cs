using UnityEngine;

namespace uSurvival
{
    // inventory, attributes etc. can influence max
    public interface INutritionBonus
    {
        short NutritionBonus(short baseNutrition);
        short NutritionRecoveryBonus();
    }

    [DisallowMultipleComponent]
    public class Nutrition : Energy
    {
        public short baseRecoveryPerTick = -1;
        public short baseNutrition = 100;

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        INutritionBonus[] _bonusComponents;
        INutritionBonus[] bonusComponents =>
            _bonusComponents ??= GetComponents<INutritionBonus>();

        public override short max
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (INutritionBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.NutritionBonus(baseNutrition);
                return (short)(baseNutrition + bonus);
            }
        }

        public override short recoveryPerTick
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (INutritionBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.NutritionRecoveryBonus();

                return (short)(baseRecoveryPerTick + bonus);
            }
        }
    }
}