using Mirror;
using System;

namespace GFFAddons
{
    [Serializable]
    public struct WeaponSkin
    {
        public string weapon;
        public string skin;

        // constructor
        public WeaponSkin(string weapon, string skin)
        {
            this.weapon = weapon;
            this.skin = skin;
        }
    }

    public class SyncListWeaponSkin : SyncList<WeaponSkin> { }
}


