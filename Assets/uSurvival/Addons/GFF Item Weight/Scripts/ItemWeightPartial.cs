using GFFAddons;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        [Header("GFF weight of the item in grams")]
        [SerializeField] private uint _itemWeight;

        public uint itemWeight => _itemWeight;

        public string GetItemWeight(uint amount)
        {
            uint weight = _itemWeight * amount;

            if (weight < 1000) return weight + "g";
            else return string.Format("{0:f1}", (float)weight / 1000) + "kg";
        }
    }

    public partial class EquipmentItem
    {
        [Header("GFF Item Weight Bonuses in grams")]
        [SerializeField] private int _weightBonus;

        public int weightBonus => _weightBonus;

        public string GetItemWeightBonusInKg()
        {
            return string.Format("{0:f1}", (_weightBonus / 1000)) + "Kg";
        }

        // tooltip
        void ToolTip_Weight(StringBuilder tip)
        {
            tip.Replace("{WEIGHTBONUS}", (_weightBonus / 100).ToString());
        }
    }

    public partial class Inventory : IWeightBonus
    {
        public int GetWeightCurrent()
        {
            int value = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0)
                    value += (int)(slot.item.data.itemWeight * slot.amount);

            return value;
        }

        public int GetWeightBonus()
        {
            return 0;
        }
    }

    public partial class Equipment : IWeightBonus
    {
        [Header("Item Weight Addon")]
        public bool useWeightAddon = false;

        public int GetWeightCurrent()
        {
            if (!useWeightAddon) return 0;

            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int value = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0)
                    value += (int)((slot.item.data.itemWeight * slot.amount) / 2);
            return value;
        }

        public int GetWeightBonus()
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.weightBonus;
            return bonus;
        }
    }

    public partial class Player
    {
        public PlayerWeight weight;
    }

    //public partial class PlayerHotbar : IWeightBonus
    //{
    //    [Header("Item Weight Addon")]
    //    public bool useWeightAddon = false;

    //    public int GetWeightCurrent()
    //    {
    //        if (!useWeightAddon) return 0;

    //        // calculate equipment bonus
    //        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
    //        int value = 0;
    //        foreach (ItemSlot slot in slots)
    //            if (slot.amount > 0 && slot.item.CheckDurability())
    //                value += (int)(slot.item.data.itemWeight * slot.amount);
    //        return value;
    //    }

    //    public int GetWeightBonus() { return 0; }
    //}

    public partial class UIStatus
    {
        [Header("GFF Addons: Weight")]
        public Text weightText;

        private void Update_Weight(Player player)
        {
            weightText.text = player.weight.CurrentWeightToString() + "/" + player.weight.CurrentWeightMaxToString();
            weightText.color = player.weight.weightCurrent >= player.weight.weightMax ? Color.red : Color.white;
        }
    }
}