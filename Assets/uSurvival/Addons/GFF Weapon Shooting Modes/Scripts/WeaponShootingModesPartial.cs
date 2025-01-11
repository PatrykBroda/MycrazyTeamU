using GFFAddons;
using UnityEngine;

namespace GFFAddons
{
    public enum ShootingModes : byte { single, burst, auto }
}

namespace uSurvival
{
    public partial class RangedWeaponItem
    {
        [Header("Weapon Shooting Modes Addon")]
        public ShootingModes[] shootingModes = new ShootingModes[1] { ShootingModes.single };
        public byte burstAmmoAmount = 3;
        public AudioClip changeShootingModeSound;
    }

    public partial class PlayerEquipment
    {
        [Header("Weapon Shooting Modes Addon")]
        public ShootingModes selectedShootingMode = ShootingModes.single;
        private int burstAmmoAmount = 0;

        public void SetShootingMode()
        {
            if (slots[selection].amount > 0 && slots[selection].item.data is RangedWeaponItem weapon)
            {
                if (weapon.shootingModes.Length > (byte)selectedShootingMode + 1) selectedShootingMode += 1;
                else selectedShootingMode = 0;
            }
        }
    }
}

