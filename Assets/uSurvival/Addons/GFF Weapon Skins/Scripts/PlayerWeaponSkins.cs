using Mirror;

namespace GFFAddons
{
    public class PlayerWeaponSkins : NetworkBehaviour
    {
        public readonly SyncListWeaponSkin skins = new SyncListWeaponSkin();
        public readonly SyncListWeaponSkin selectedskins = new SyncListWeaponSkin();

        public int GetSelectedSkinIndex(string weapon)
        {
            for (int i = 0; i < selectedskins.Count; i++)
            {
                if (selectedskins[i].weapon == weapon) return i;
            }

            return -1;
        }

        [Command]
        public void CmdUseSkin(string weapon, string skin)
        {
            int weaponIndex = GetSelectedSkinIndex(weapon);
            if (weaponIndex == -1)
            {
                selectedskins.Add(new WeaponSkin(weapon, skin));
            }
            else
            {
                if (selectedskins[weaponIndex].skin != skin) selectedskins.Add(new WeaponSkin(weapon, skin));
                selectedskins.RemoveAt(weaponIndex);
            }
        }

        public bool IsSkinAlreadyPurchased(string weapon, string skin)
        {
            for (int i = 0; i < skins.Count; i++)
            {
                if (skins[i].weapon == weapon && skins[i].skin == skin) return true;
            }

            return false;
        }
    }
}


