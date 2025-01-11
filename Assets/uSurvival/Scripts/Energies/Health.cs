using UnityEngine;

namespace uSurvival
{
    // inventory, attributes etc. can influence max health
    public interface IHealthBonus
    {
        short HealthBonus(short baseHealth);
        short HealthRecoveryBonus();
    }

    [DisallowMultipleComponent]
    public class Health : Energy
    {
        public short baseRecoveryPerTick = 0;
        public short baseHealth = 100;

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        IHealthBonus[] _bonusComponents;
        IHealthBonus[] bonusComponents =>
            _bonusComponents ??= GetComponents<IHealthBonus>();

        public override short max
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (IHealthBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.HealthBonus(baseHealth);

                return (short)(baseHealth + bonus);
            }
        }

        public override short recoveryPerTick
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (IHealthBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.HealthRecoveryBonus();

                return (short)(baseRecoveryPerTick + bonus);
            }
        }
    }
}