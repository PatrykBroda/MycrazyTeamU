using GFFAddons;
using UnityEngine;
using uSurvival;

namespace uSurvival
{
    public partial class Player
    {
        [Header("GFF Crafting Extended Addon")]
        public PlayerCraftingExtended craftingExtended;
    }
}

namespace GFFAddons
{
    public partial class UIInventoryExtended
    {
        void IsUsedSlot_CraftExtended(Player player, int inventoryIndex)
        {
            if (isUsedSlot == false)
            {
                if (UICraftingExtended.singleton.panel.activeSelf)
                {
                    for (int i = 0; i < player.craftingExtended.craftingIndices.Count; i++)
                    {
                        if (inventoryIndex == player.craftingExtended.craftingIndices[i])
                        {
                            isUsedSlot = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}