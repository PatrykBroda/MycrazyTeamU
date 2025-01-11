using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using GFFAddons;

namespace uSurvival
{
    // inventory, attributes etc. can influence max health
    public interface ICombatBonus
    {
        short DamageBonus();
        short DefenseBonus();

        //gff
        float GetRadiationResistanseBonus();
    }

    [Serializable] public class UnityEventInt : UnityEvent<int> {}

    public partial class Combat : NetworkBehaviour
    {
        [Header("Components")]
        public Entity entity;

        [Header("Stats")]
        public int baseDamage;
        public int baseDefense;
        public GameObject onDamageEffect;

        // it's useful to know an entity's last combat time (did/was attacked)
        // e.g. to prevent logging out for x seconds after combat
        [SyncVar] public double lastCombatTime;

        // events
        public UnityEventEntityInt onServerReceivedDamage;
        public UnityEventInt onClientReceivedDamage;

        // cache components that give a bonus (attributes, inventory, etc.)
        ICombatBonus[] bonusComponents;
        void Awake()
        {
            bonusComponents = GetComponentsInChildren<ICombatBonus>();
        }

        // calculate damage
        public int damage
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                int bonus = 0;
                foreach (ICombatBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.DamageBonus();

                int weaponBonus = 0;
                if (entity is Player player && player.equipment.IsHandsOrItemWithValidDurability(player.equipment.selection))
                {
                    ItemSlot slot = player.equipment.slots[player.equipment.selection];
                    if (slot.amount > 0 && slot.item.data is WeaponItem weapon) weaponBonus = weapon.damage;
                    else weaponBonus = 5;
                }

                //return baseDamage + bonus + weaponBonus;

                if (entity is Player pl) return baseDamage + bonus + weaponBonus + pl.GetDamageBonus_CombatSkills(weaponBonus);                
                else return baseDamage + bonus + weaponBonus;
            }
        }

        // calculate defense
        public int defense
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                int bonus = 0;
                foreach (ICombatBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.DefenseBonus();

                int suitBonus = 0;
                if (entity is Player player)
                {
                    //EquipmentItem suit = player.customization.GetSuitData();
                    //suitBonus = suit.defenseBonus;
                }

                int _defense = baseDefense + bonus + suitBonus;

                if (entity is Player pl) return _defense + pl.GetDefenseBonus_CombatSkills(_defense);
                else return _defense;

                //return baseDefense + bonus + suitBonus;
            }
        }

        // deal damage while acknowledging the target's defense etc.
        public void DealDamageAt(Entity victim, int amount, Vector3 hitPoint, Vector3 hitNormal, Collider hitCollider)
        {
            if (victim is Npc) return;

            Combat victimCombat = victim.combat;

            // not dead yet?
            if (victim.health.current > 0)
            {
                // extra damage on that collider? (e.g. on head)
                DamageArea damageArea = hitCollider.GetComponent<DamageArea>();
                float multiplier = damageArea != null ? damageArea.multiplier : 1;

                //check equipment armor
                if (victim is Player victiimPlayer)
                {
                    if (damageArea != null)
                    {
                        float multiplierReducingDamage = GetEquipmentSlotByIndex(victiimPlayer, damageArea.checkingEquipmentSlots);
                        multiplier -= multiplierReducingDamage;
                        if (multiplier < 0) multiplier = 0.1f;
                    }
                }

                int amountMultiplied = Mathf.RoundToInt(amount * multiplier);

                if (victim is Player pl && pl.currentSafeZoneSource != null) return;
                if (entity is Player pl2 && pl2.currentSafeZoneSource != null) return;

                // subtract defense (but leave at least 1 damage, otherwise
                // it may be frustrating for weaker players)
                short damageDealt = (short)Mathf.Max(amountMultiplied - victimCombat.defense, 1);

                // deal the damage
                victim.health.current -= damageDealt;

                if (entity is Player player)
                {
                    //gm tool addon
                    if (player.isGameMaster && player.gameMasterTool.killingWithOneHit) victim.health.current = 0;

                    if (victim.health.current <= 0)
                    {
                        if (victim is Player victimPlayer)
                        {
                            player.statistics.playersKilled++;
                            RpcShowPlayerKill(player.name, victim.name);

                            SetClans(player, victimPlayer);
                        }
                        else if (victim is Monster) player.statistics.monstersKilled++;
                    }

                    TargetShowOutgoingDamageInfo(damageDealt);
                    OnServerHitEnemyCombatSkills.Invoke(victim, damageDealt);
                }
                else if (victim is Player)
                {
                    victimCombat.TargetShowIncomingDamageInfo(damageDealt);
                    victimCombat.OnServerReceivedDamageCombatSkills.Invoke();
                }

                // call OnServerReceivedDamage event on the target
                // -> can be used for monsters to pull aggro
                // -> can be used by equipment to decrease durability etc.
                victimCombat.onServerReceivedDamage.Invoke(entity, damageDealt);

                // show effects on clients
                victimCombat.RpcOnReceivedDamage(damageDealt, hitPoint, hitNormal);

                // reset last combat time for both
                lastCombatTime = NetworkTime.time;
                victimCombat.lastCombatTime = NetworkTime.time;

                // call OnDamageDealtTo / OnKilledEnemy events
                onDamageDealtTo.Invoke(victim);
                if (victim.health.current == 0)
                    onKilledEnemy.Invoke(victim);
            }
        }

        [ClientRpc]
        public void RpcOnReceivedDamage(int amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            // show damage effect (if any)
            if (onDamageEffect)
                Instantiate(onDamageEffect, hitPoint, Quaternion.LookRotation(-hitNormal));

            // call OnClientReceivedDamage event
            onClientReceivedDamage.Invoke(amount);
        }

        private float GetEquipmentSlotByIndex(Player player, int[] checkingSlots)
        {
            float multiplier = 0;
            for (int i = 0; i < checkingSlots.Length; i++)
            {
                if (player.equipment.slots[checkingSlots[i]].amount > 0 && player.equipment.slots[checkingSlots[i]].item.durability > 0 && player.equipment.slots[checkingSlots[i]].item.data is EquipmentItem eItem)
                {
                    multiplier += eItem.multiplierReducingDamage; 
                }
            }

            return multiplier;
        }
    }
}