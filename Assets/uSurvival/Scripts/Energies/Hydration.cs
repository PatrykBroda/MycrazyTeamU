using UnityEngine;

namespace uSurvival
{
    // inventory, attributes etc. can influence max
    public interface IHydrationBonus
    {
        short HydrationBonus(short baseHydration);
        short HydrationRecoveryBonus();
    }

    [DisallowMultipleComponent]
    public class Hydration : Energy
    {
        public short baseRecoveryPerTick = -1;
        public short baseHydration = 100;

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        IHydrationBonus[] _bonusComponents;
        IHydrationBonus[] bonusComponents =>
            _bonusComponents ??= GetComponents<IHydrationBonus>();

        public override short max
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (IHydrationBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.HydrationBonus(baseHydration);
                return (short)(baseHydration + bonus);
            }
        }

        public override short recoveryPerTick
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (IHydrationBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.HydrationRecoveryBonus();

                return (short)(baseRecoveryPerTick + bonus);
            }
        }
    }
}