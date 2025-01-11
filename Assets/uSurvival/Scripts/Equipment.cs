using UnityEngine;

namespace uSurvival
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Inventory))]
    public abstract partial class Equipment : ItemContainer, IHealthBonus, IHydrationBonus, INutritionBonus, ICombatBonus
    {
        // Used components. Assign in Inspector. Easier than GetComponent caching.
        public Health health;
        public Inventory inventory;

        // energy boni
        public short HealthBonus(short baseHealth)
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            short bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.healthBonus;
            return bonus;
        }
        public short HealthRecoveryBonus() => 0;
        public short HydrationBonus(short baseHydration)
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            short bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.hydrationBonus;
            return bonus;
        }
        public short HydrationRecoveryBonus() => 0;
        public short NutritionBonus(short baseNutrition)
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            short bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.nutritionBonus;
            return bonus;
        }
        public short NutritionRecoveryBonus() => 0;

        // combat boni
        public short DamageBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            short bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.damageBonus;
            return bonus;
        }
        public short DefenseBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            short bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.defenseBonus;
            return bonus;
        }
    }
}