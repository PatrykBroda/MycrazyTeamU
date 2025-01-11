using GFFAddons;
using System;
using UnityEngine;

namespace GFFAddons
{
    [Serializable]
    public struct WeaponSkinData
    {
        [HideInInspector] public string name;
        public GameObject prefab;
        public Sprite sprite;
    }
}

namespace uSurvival
{
    public partial class ScriptableItem
    {
        [Header("Weapon Skins")]
        public WeaponSkinData[] skins;

        public GameObject GetModel(int weaponIndex, string selectedskin)
        {
            if (skins != null && skins.Length > 0 && weaponIndex != -1)
            {
                for (int i = 0; i < skins.Length; i++)
                {
                    if (skins[i].prefab.name == selectedskin) return skins[i].prefab;
                }
            }
            return modelPrefab;
        }

        public Sprite GetWeaponSkinSprite(int weaponIndex, string selectedskin)
        {
            if (skins != null && skins.Length > 0 && weaponIndex != -1)
            {
                for (int i = 0; i < skins.Length; i++)
                {
                    if (skins[i].prefab.name == selectedskin) return skins[i].sprite;
                }
            }
            return null;
        }
    }

    public partial class Database
    {
        class weaponSkins
        {
            public string account { get; set; }
            public string weapon { get; set; }
            public string skin { get; set; }
        }

        class weaponSelectedSkins
        {
            public string owner { get; set; }
            public string weapon { get; set; }
            public string skin { get; set; }
        }

        public void Connect_WeaponSkins()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<weaponSkins>();
            connection.CreateIndex(nameof(weaponSkins), new[] { "account", "weapon" });
            connection.CreateTable<weaponSelectedSkins>();
            connection.CreateIndex(nameof(weaponSelectedSkins), new[] { "owner", "weapon" });
        }

        public void CharacterLoad_WeaponSkins(Player player)
        {
            foreach (weaponSkins row in connection.Query<weaponSkins>("SELECT * FROM weaponSkins WHERE account =?", player.account))
            {
                if (ScriptableItem.dict.TryGetValue(row.weapon.GetStableHashCode(), out ScriptableItem itemData))
                {
                    player.weaponSkins.skins.Add(new WeaponSkin(row.weapon, row.skin));
                }
            }
        }

        public void CharacterSave_WeaponSkins(Player player)
        {
            connection.Execute("DELETE FROM weaponSkins WHERE account=?", player.account);

            for (int i = 0; i < player.weaponSkins.skins.Count; ++i)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new weaponSkins
                {
                    account = player.account,
                    weapon = player.weaponSkins.skins[i].weapon,
                    skin = player.weaponSkins.skins[i].skin
                });
            }

            CharacterSave_WeaponSelectedSkins(player);
        }

        private void CharacterLoad_WeaponSelectedSkins(Player player)
        {
            foreach (weaponSelectedSkins row in connection.Query<weaponSelectedSkins>("SELECT * FROM weaponSelectedSkins WHERE owner =?", player.name))
            {
                if (ScriptableItem.dict.TryGetValue(row.weapon.GetStableHashCode(), out ScriptableItem itemData))
                {
                    player.weaponSkins.selectedskins.Add(new WeaponSkin(row.weapon, row.skin));
                }
            }
        }

        private void CharacterSave_WeaponSelectedSkins(Player player)
        {
            connection.Execute("DELETE FROM weaponSelectedSkins WHERE owner=?", player.name);

            for (int i = 0; i < player.weaponSkins.selectedskins.Count; ++i)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new weaponSelectedSkins
                {
                    owner = player.name,
                    weapon = player.weaponSkins.selectedskins[i].weapon,
                    skin = player.weaponSkins.selectedskins[i].skin
                });
            }
        }
    }

    public partial class Player
    {
        public PlayerWeaponSkins weaponSkins;
    }
}



