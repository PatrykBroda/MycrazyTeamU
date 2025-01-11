using GFFAddons;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace uSurvival
{
    public partial class EquipmentItem
    {
        [Header("GFF Combat Skills")]
        public CombatSkillItem requiredCombatSkill;
        public int requiredLevel = 1;

        private bool CheckCombatSkills(Player player)
        {
            if (requiredCombatSkill == null) return true;

            for (int i = 0; i < player.combatSkills.skillTemplates.Length; i++)
            {
                if (player.combatSkills.skillTemplates[i].Equals(requiredCombatSkill))
                {
                    return player.combatSkills.skills[i].level >= requiredLevel;
                }
            }
            return false;
        }

        // tooltip
        private void ToolTip_CombatSkills(StringBuilder tip)
        {
            if (requiredCombatSkill != null)
                tip.Replace("{COMBATSKILL}", requiredCombatSkill.name + ": " + requiredLevel);
        }
    }

    public partial class Player
    {
        [Header("GFF Combat Skills")]
        public PlayerCombatSkills combatSkills;

        //health
        public short BonusHealth_CombatSkills()
        {
            short bonus = 0;

            for (int i = 0; i < combatSkills.skills.Count; i++)
            {
                //fixed amount
                if (combatSkills.skillTemplates[i].bonusHealth > 0 && combatSkills.skills[i].level > 1)
                    bonus += (short)(combatSkills.skillTemplates[i].bonusHealth * (combatSkills.skills[i].level - 1));

                //percent
                if (combatSkills.skillTemplates[i].bonusHealthPercent > 0 && combatSkills.skills[i].level > 1)
                    bonus += (short)(health.baseHealth * ((combatSkills.skills[i].level - 1) * combatSkills.skillTemplates[i].bonusHealthPercent));
            }

            return bonus;
        }

        //damage
        public int GetDamageBonus_CombatSkills(int damageAndEequipmentBonus)
        {
            int bonus = 0;

            //find weapon in equipment
            if (equipment.slots[equipment.selection].amount > 0 && equipment.slots[equipment.selection].item.data is WeaponItem weapon)
            {
                //melee weapon
                if (weapon is MeleeWeaponItem)
                {
                    for (int i = 0; i < combatSkills.skills.Count; i++)
                    {
                        if (combatSkills.skillTemplates[i].increaseExp == IncreaseExpType.MeleeDamage)
                        {
                            //percent
                            if (combatSkills.skillTemplates[i].bonusDamageMeleePercent > 0)
                                bonus += (short)(damageAndEequipmentBonus * (float)(combatSkills.skills[i].level * combatSkills.skillTemplates[i].bonusDamageMeleePercent));
                        }
                    }
                }

                //range weapon
                else
                {
                    for (int i = 0; i < combatSkills.skills.Count; i++)
                    {
                        if (combatSkills.skillTemplates[i].increaseExp == IncreaseExpType.RangeDamage)
                        {
                            //percent
                            if (combatSkills.skillTemplates[i].bonusDamageRangePercent > 0)
                            {
                                bonus += (int)(damageAndEequipmentBonus * (float)(combatSkills.skills[i].level * combatSkills.skillTemplates[i].bonusDamageRangePercent));
                            }
                        }
                    }
                }
            }

            return bonus;
        }

        //defense
        public short GetDefenseBonus_CombatSkills(int defenses)
        {
            short bonus = 0;

            for (int i = 0; i < combatSkills.skills.Count; i++)
            {
                //percent
                if (combatSkills.skillTemplates[i].bonusDefensePercent > 0)
                    bonus += (short)(defenses * ((combatSkills.skills[i].level) * combatSkills.skillTemplates[i].bonusDefensePercent));
            }

            return bonus;
        }
    }

    public partial class Combat
    {
        [Header("GFF Combat Skills Events")]
        public UnityEvent OnServerReceivedDamageCombatSkills;
        public UnityEventEntityInt OnServerHitEnemyCombatSkills;
    }
}


public partial class UICharacterInfoExtended
{
    /* [Header("Settings: Combat Skill")]
     public bool limitToCharacterLevel;
     public bool limitToCharacterClass;
     public int minTargetLevel = 1;
     public int maxTargetLevel = 1;*/

    //void Start_CombatSkills()
    //{
    //    panelCombatSkills.SetActive(true);
    //}

    //void Update_CombatSkills(Player player)
    //{
    //    if (player.combatSkills.skills.Count > 0)
    //    {
    //        // instantiate/destroy enough slots
    //        UIUtils.BalancePrefabs(combatSkillsPrefab, player.combatSkills.skillTemplates.Length, combatSkillsContentForPrefabs);

    //        for (int i = 0; i < player.combatSkills.skillTemplates.Length; i++)
    //        {
    //            UICombatSkillSlot slot = combatSkillsContentForPrefabs.GetChild(i).GetComponent<UICombatSkillSlot>();

    //            slot.textName.text = player.combatSkills.skillTemplates[i].name;
    //            slot.sliderExp.value = player.combatSkills.skills[i].GetPercent();
    //            slot.textPercent.text = (slot.sliderExp.value * 100).ToString("F") + "%";
    //            slot.textLevel.text = player.combatSkills.skills[i].level + " lv";
    //        }
    //    }
    //}
}

/*public partial class Stamina
{
    int UpdateBonus_CombatSkill(Entity owner, int staminaValue)
    {
        int bonus = 0;
        if (owner is Player player)
        {
            //melee
            if (player.combatSkills.melee > 1)
            {
                //fixed amount
                bonus += (player.combatSkills.melee - 1) * UICharacterInfoExtended.singleton.ListFightingSkill[0].bonusStamina;

                //percent
                bonus += Convert.ToInt32(staminaValue * ((player.combatSkills.melee - 1) * UICharacterInfoExtended.singleton.ListFightingSkill[0].bonusStaminaPercent));
            }
            if (player.combatSkills.range > 1)
            {
                //fixed amount
                bonus += (player.combatSkills.range - 1) * UICharacterInfoExtended.singleton.ListFightingSkill[1].bonusStamina;

                //percent
                bonus += Convert.ToInt32(staminaValue * (player.combatSkills.range * UICharacterInfoExtended.singleton.ListFightingSkill[1].bonusStaminaPercent));
            }
        }

        return bonus;
    }
}*/
