using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerCombatSkills : NetworkBehaviour
    {
        [Header("Components")] // to be assigned in inspector
        public Player player;
        public PlayerEquipment equipment;

        public CombatSkillItem[] skillTemplates;
        [HideInInspector] public readonly SyncListCombatSkill skills = new SyncListCombatSkill();

        [Header("Settings")]
        public bool useMinDamageForCombatSkills;
        public float minDamageForCombatSkills = 0;

        [SerializeField] private int getExp = 10;

        public void IncreaseExp(IncreaseExpType type)
        {
            Debug.Log("IncreaseExp " + type.ToString());

            for (int i = 0; i < skillTemplates.Length; i++)
            {
                if (skillTemplates[i].increaseExp == type && skills[i].level < skillTemplates[i].maxLevel)
                {
                    CombatSkill skill = skills[i];

                    if (skill.exp + getExp < skill._experienceMax.Get(skill.level)) skill.exp += getExp;
                    else
                    {
                        if (skill.exp + getExp == skill._experienceMax.Get(skill.level))
                        {
                            skill.level++;
                            skill.exp = 0;
                        }
                        else
                        {
                            long temp = skill._experienceMax.Get(skill.level) - skill.exp;
                            skill.level++;
                            skill.exp = getExp - temp;
                        }
                    }

                    skills[i] = skill;
                }
            }
        }

        public void OnServerReceivedDamage_CombatSkills()
        {
            //defense
            player.combatSkills.IncreaseExp(IncreaseExpType.GetHit);
        }

        public void OnHitEnemy_CombatSkills(Entity target, int amount)
        {
            Debug.Log("OnHitEnemy");
            //in order for the points to be counted, it is necessary that the damage is not lower than the minimum
            if (useMinDamageForCombatSkills == false || amount >= (target.health.max / 100) * minDamageForCombatSkills)
            {
                //find weapon in equipment
                if (equipment.slots[equipment.selection].amount > 0 && equipment.slots[equipment.selection].item.data is WeaponItem weapon)
                {
                    //melee
                    if (weapon is MeleeWeaponItem)
                    {
                        IncreaseExp(IncreaseExpType.MeleeDamage);
                    }

                    //range
                    else if (weapon is RangedWeaponItem)
                    {
                        IncreaseExp(IncreaseExpType.RangeDamage);
                    }
                }
                else IncreaseExp(IncreaseExpType.useHands);
            }
        }
    }
}


