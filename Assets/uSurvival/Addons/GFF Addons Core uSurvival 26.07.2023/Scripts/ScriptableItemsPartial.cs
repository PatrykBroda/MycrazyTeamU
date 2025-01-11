using GFFAddons;
using System;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public enum ItemType { Armor, Weapon, Potions, Ammo, Resources, Accessories, Pets, Mounts }
    public enum CharacterArmorType { None, Helmet, Shoulders, Upper, Lower, Gloves, Shoes, Shield };
    public enum EquipmentItemType { Head, Hair, Beard, Hats, Glasses, Masks, Jackets, Pants, Boots, Gloves, Suit, Backpacks, Bodyarmor, ShoulderL, ShoulderR, KneepaL, KneepaR, Pouch, SkinColor, Pullover }
    public enum MountArmorType { None, Shoulders, Helmet, Upper, Lower, Gloves, Shoes };
    public enum WeaponType { None, Knife, Sword, Axe, Mace, Staff, Spear, Bow, Crossbow, Throwing, Shield };
    public enum PotionType { Medicine, Food, Drink, pets, mounts };

    public enum BuffType { Normal, Doping, Single, Double, Totem, Exp, Health, Mana, Stamina, Damage, Defense, Crit, Block, Dodge, Accuracy }

    //for gathering addon
    [Serializable]
    public class ScriptableItemAndRandomAmount
    {
        public ScriptableItem item;
        public int amountMin = 1;
        public int amountMax = 1;
        public int weight = 1;
    }
}

namespace uSurvival
{
    public partial class ScriptableItem
    {
        public string GetItemCategory()
        {
            if (this is PotionItem) return "Potion";
            else if (this is AmmoItem) return "Ammo";
            else if (this is WeaponItem) return "Weapon";
            else if (this is EquipmentItem) return "Armor";
            else return "Resources";
        }

        public string GetItemSubCategory()
        {
            if (this is WeaponItem weapon)
            {
                return weapon.weaponType.ToString();
            }
            else if (this is PotionItem potion)
            {
                if (potion.potionType == PotionType.pets || potion.potionType == PotionType.mounts)
                    return potion.potionType.ToString();
                else return "Character";
            }
            //else if (this is AccessoryItem accessory)
            //{
            //    return accessory.accessoryType.ToString();
            //}
            else if (this is EquipmentItem armor)
            {
                return armor.armorType.ToString();
            }

            return "";
        }
    }

    public partial class EquipmentItem
    {
        [Header("GFF Equipment Type")]
        public CharacterArmorType armorType;
        public float multiplierReducingDamage;
    }

    public partial class WeaponItem
    {
        [Header("GFF Weapon Type")]
        public WeaponType weaponType;
    }

    public partial class PotionItem
    {
        [Header("GFF Potion Type")]
        public PotionType potionType;
    }
}
