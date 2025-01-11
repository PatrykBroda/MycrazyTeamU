using System.Text;
using UnityEngine;

namespace uSurvival
{
    [CreateAssetMenu(menuName="uSurvival Item/Potion", order=999)]
    public partial class PotionItem : UsableItem
    {
        [Header("Potion")]
        public short usageHealth;
        public short usageHydration;
        public short usageNutrition;

        // note: no need to overwrite CanUse functions. simply check cooldowns in base.

        void ApplyEffects(Player player)
        {
            player.health.current += usageHealth;
            player.hydration.current += usageHydration;
            player.nutrition.current += usageNutrition;

            //gff
            player.boosts.boost = (byte)(player.boosts.boost + usageBoost) < 100 ? (byte)(player.boosts.boost + usageBoost) : (byte)100;
        }

        public override void UseInventory(Player player, int inventoryIndex)
        {
            // call base function to start cooldown
            base.UseInventory(player, inventoryIndex);

            ApplyEffects(player);

            // decrease amount
            ItemSlot slot = player.inventory.slots[inventoryIndex];
            slot.DecreaseAmount(1);
            player.inventory.slots[inventoryIndex] = slot;
        }

        //public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
        //{
        //    // call base function to start cooldown
        //    base.UseHotbar(player, hotbarIndex, lookAt);

        //    ApplyEffects(player);

        //    // decrease amount
        //    ItemSlot slot = player.equipment.slots[hotbarIndex];
        //    slot.DecreaseAmount(1);
        //    player.equipment.slots[hotbarIndex] = slot;
        //}

        public override void UseEquipment(Player player, int equipmentIndex)
        {
            // call base function to start cooldown
            base.UseEquipment(player, equipmentIndex);

            ApplyEffects(player);

            // decrease amount
            ItemSlot slot = player.equipment.slots[equipmentIndex];
            slot.DecreaseAmount(1);
            player.equipment.slots[equipmentIndex] = slot;
        }

        //public override void OnUsedInventory(Player player)
        //{
        //    base.OnUsedInventory(player);

        //    if (successfulUseSound != null) player.audioSource.PlayOneShot(successfulUseSound);
        //}

        // tooltip
        public override string ToolTip()
        {
            StringBuilder tip = new StringBuilder(base.ToolTip());
            tip.Replace("{USAGEHEALTH}", usageHealth.ToString());
            tip.Replace("{USAGEHYDRATION}", usageHydration.ToString());
            tip.Replace("{USAGENUTRITION}", usageNutrition.ToString());
            return tip.ToString();
        }
    }
}