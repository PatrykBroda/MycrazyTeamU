using GFFAddons;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace uSurvival
{
    public partial class EquipmentItem
    {
        [Header("GFF Radiation Bonus")]
        [SerializeField, Range(0, 1)] private float _radiationResistanceBonus;
        public float radiationResistanceBonus => _radiationResistanceBonus;

        // tooltip
        private void ToolTip_RadiationResistance(StringBuilder tip)
        {
            tip.Replace("{RADIATIONRESISTANCEBONUS}", radiationResistanceBonus.ToString());
        }
    }

    public partial class PotionItem
    {
        [Header("Radiation")]
        [SerializeField] private int _usageRadiation;
        public int usageRadiation => _usageRadiation;
    }

    public partial class Player
    {
        [Header("Components")]
        public Radiation radiation;
    }

    public partial class Combat
    {
        [Header("Components")]
        public Radiation radiation;

        public float radiationResistance
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                float resistance = 0;
                foreach (ICombatBonus bonusComponent in bonusComponents)
                    resistance += bonusComponent.GetRadiationResistanseBonus();

                if (entity is Player player)
                {
                    EquipmentItem suit = player.customization.GetSuitData();
                    resistance += suit.radiationResistanceBonus;
                }

                if (resistance > 1) resistance = 1;
                return radiation.defaultRadiationResistance + resistance;
            }
        }
    }

    public abstract partial class Equipment
    {
        public float GetRadiationResistanseBonus()
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem && ((eItem.secondItems == null || eItem.secondItems.Length == 0) || slot.item.secondItemDurability > 0))
                {
                    bonus += eItem.radiationResistanceBonus;
                }

            return bonus;
        }
    }

    public partial class PlayerHotbar
    {
        public float GetRadiationResistanseBonus() { return 0; }
    }

    public partial class UIStatus
    {
        [Header("Radiation")]
        public Text radiationStatus;
    }
}