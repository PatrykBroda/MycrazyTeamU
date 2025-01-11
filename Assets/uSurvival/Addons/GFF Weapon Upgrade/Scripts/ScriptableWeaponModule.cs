using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "uSurvival Item/Weapon Module", order = 999)]
    public class ScriptableWeaponModule : UsableItem
    {
        [Header("Weapon Module type")]
        public WeaponModuleType moduleType;

        [Header("For Barrels modules")]
        public bool muffle = false;
        public bool flashHider = false;

        [Header("For Sights modules")]
        public float zoom = 0;
        public Sprite sightsImage;

        [Header("For Magazine modules")]
        public ushort addMagazinAmmo = 0;
        [Range(0, 1)] public float reducesReloadTime = 0;

        // usage
        public override Usability CanUseInventory(Player player, int inventoryIndex)
        {
            return Usability.Usable;
        }

        public override void UseInventory(Player player, int inventoryIndex)
        {
            player.EquipModuleByClick(inventoryIndex);
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