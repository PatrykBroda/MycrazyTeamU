using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "GFF Addons/Crafting Extended/Studyable Scroll", order = 999)]
    public class StudyableScroll : UsableItem
    {
        public override Usability CanUseInventory(Player player, int inventoryIndex)
        {
            // check base usability first (cooldown etc.)
            Usability baseUsable = base.CanUseInventory(player, inventoryIndex);
            if (baseUsable != Usability.Usable) return baseUsable;

            return CheckRecipes(player) == true
                   ? Usability.Usable
                   : Usability.Never;
        }

        public override void UseInventory(Player player, int inventoryIndex)
        {
            // call base function to start cooldown
            base.UseInventory(player, inventoryIndex);

            player.craftingExtended.learnedCraftingRecipes.Add(player.inventory.slots[inventoryIndex].item.name);

            // decrease amount
            //ItemSlot slot = player.inventory[inventoryIndex];
            //slot.amount = 0;
            player.inventory.slots[inventoryIndex] = new ItemSlot();
        }

        bool CheckRecipes(Player player)
        {
            //search by name among the recipes learned
            for (int i = 0; i < player.craftingExtended.learnedCraftingRecipes.Count; i++)
            {
                if (player.craftingExtended.learnedCraftingRecipes[i] == name) return false;
            }
            return true;
        }
    }
}


