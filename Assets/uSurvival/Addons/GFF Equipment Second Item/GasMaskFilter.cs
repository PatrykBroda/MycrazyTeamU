using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "GasMaskFilter Item", order = 999)]
    public class GasMaskFilter : UsableItem
    {
        [Header("Gas Mask Filter")]
        public float time;

        // usage
        public override Usability CanUseInventory(Player player, int inventoryIndex)
        {
            return Usability.Usable;
        }

        public override void UseInventory(Player player, int inventoryIndex)
        {
            player.equipment.EquipSecondSlotByClick(inventoryIndex);
        }

        public override void OnUsedInventory(Player player)
        {
            base.OnUsedInventory(player);

            if (puttingOnSound != null) player.audioSource.PlayOneShot(puttingOnSound);
        }

        public override void OnUsedEquipment(Player player)
        {
            base.OnUsedEquipment(player);

            if (removedSound != null) player.audioSource.PlayOneShot(removedSound);
        }
    }
}