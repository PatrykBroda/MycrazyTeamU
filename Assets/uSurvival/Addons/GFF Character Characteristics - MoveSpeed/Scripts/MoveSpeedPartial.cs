using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace uSurvival
{
    public interface IMoveSpeedBonus
    {
        float GetMoveSpeedBonus();

        float GetSpeedIncreasePercentageBonus();
        float GetSpeedDecreasePercentageBonus();
    }

    public partial class EquipmentItem
    {
        [Header("GFF Move Speed Bonus")]
        public float moveSpeedBonus = 0;
        public float moveSpeedBonusPercentage = 0;

        // tooltip
        void ToolTip_MoveSpeed(StringBuilder tip)
        {
            tip.Replace("{MOVESPEEDBONUS}", moveSpeedBonus.ToString());
            tip.Replace("{MOVESPEEDBONUSPERCENTAGE}", moveSpeedBonusPercentage.ToString());
        }
    }

    public partial class UIStatus
    {
        [Header("Settings: Move Speed Addon")]
        public Text textMoveSpeedValue;
    }

    public partial class PlayerEquipment : IMoveSpeedBonus
    {
        public PlayerMovement movement;

        public float GetMoveSpeedBonus()
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.moveSpeedBonus;
            return bonus;
        }

        public float GetSpeedIncreasePercentageBonus()
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float bonus = 0;
            foreach (ItemSlot slot in slots)
                if (slot.amount > 0 && slot.item.CheckDurability() && slot.item.data is EquipmentItem eItem)
                    bonus += eItem.moveSpeedBonusPercentage;
            return bonus;
        }

        public float GetSpeedDecreasePercentageBonus()
        {
            return 0;
        }
    }

    public partial class PlayerMovement
    {
        public Player player;

        // cache components that give a bonus (attributes, inventory, etc.)
        IMoveSpeedBonus[] moveSpeedBonusComponents;

        public float moveSpeed
        {
            get
            {
                bool runRequested = !UIUtils.AnyInputActive() && Input.GetKey(runKey);
                float _speed = runRequested && endurance.current > 0 ? runSpeed : walkSpeed;
                if (player.isGameMaster && player.gameMasterTool.superSpeed) return _speed = _speed * player.gameMasterTool.superSpeedBonus;

                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                float bonus = 0;
                float bonusIncreaseInPercent = 0;
                float bonusDecreaseInPercent = 1;
                foreach (IMoveSpeedBonus bonusComponent in moveSpeedBonusComponents)
                {
                    bonus += bonusComponent.GetMoveSpeedBonus();
                    bonusIncreaseInPercent += bonusComponent.GetSpeedIncreasePercentageBonus();

                    bonusDecreaseInPercent -= bonusComponent.GetSpeedDecreasePercentageBonus();
                }

                if (bonusDecreaseInPercent <= 0) bonusDecreaseInPercent = 0.1f;

                return (_speed + bonus + (_speed * (bonusIncreaseInPercent / 100))) * bonusDecreaseInPercent;
            }
        }

        public float crouchSpeedExtended
        {
            get
            {
                float _speed = crouchSpeedDefault;
                if (player.isGameMaster && player.gameMasterTool.superSpeed) return _speed = _speed * player.gameMasterTool.superSpeedBonus;

                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                float bonus = 0;
                float bonusIncreaseInPercent = 0;
                float bonusDecreaseInPercent = 1;
                foreach (IMoveSpeedBonus bonusComponent in moveSpeedBonusComponents)
                {
                    bonus += bonusComponent.GetMoveSpeedBonus();
                    bonusIncreaseInPercent += bonusComponent.GetSpeedIncreasePercentageBonus();

                    bonusDecreaseInPercent -= bonusComponent.GetSpeedDecreasePercentageBonus();
                }

                if (bonusDecreaseInPercent <= 0) bonusDecreaseInPercent = 0.1f;

                return (_speed + bonus + (_speed * (bonusIncreaseInPercent / 100))) * bonusDecreaseInPercent;
            }
        }

        public float crawlSpeedExtended
        {
            get
            {
                float _speed = crawlSpeedDefault;
                if (player.isGameMaster && player.gameMasterTool.superSpeed) return _speed = _speed * player.gameMasterTool.superSpeedBonus;

                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                float bonus = 0;
                float bonusIncreaseInPercent = 0;
                float bonusDecreaseInPercent = 1;
                foreach (IMoveSpeedBonus bonusComponent in moveSpeedBonusComponents)
                {
                    bonus += bonusComponent.GetMoveSpeedBonus();
                    bonusIncreaseInPercent += bonusComponent.GetSpeedIncreasePercentageBonus();

                    bonusDecreaseInPercent -= bonusComponent.GetSpeedDecreasePercentageBonus();
                }

                if (bonusDecreaseInPercent <= 0) bonusDecreaseInPercent = 0.1f;

                return (_speed + bonus + (_speed * (bonusIncreaseInPercent / 100))) * bonusDecreaseInPercent;
            }
        }
    }
}