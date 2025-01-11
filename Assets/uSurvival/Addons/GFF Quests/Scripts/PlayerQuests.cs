using Mirror;
using System;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerQuests : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;
        public PlayerInventory inventory;

        [Header("Quests")] // contains active and completed quests (=all)
        public int activeQuestLimit = 10;
        public readonly SyncListQuest quests = new SyncListQuest();

        public override void OnStartServer()
        {
            InvokeRepeating(nameof(CheckDailyQuests), 1, 5);
        }

        private void CheckDailyQuests()
        {
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].completed && quests[i].cooldown > 0 && quests[i].lastTimeCompleted.AddMinutes(quests[i].cooldown) < DateTime.Now)
                {
                    quests.RemoveAt(i);
                }
            }
        }

        [Command]
        public void CmdCompleteQuestExtended(int npcQuestIndex, int rewardId)
        {
            // validate
            // use collider point(s) to also work with big entities
            if (player.health.current > 0 &&
                player.interactionExtended.target != null && player.interactionExtended.target is Npc npc &&
                0 <= npcQuestIndex && npcQuestIndex < npc.quests.quests.Length &&
                uSurvival.Utils.ClosestDistance(player.collider, npc.collider) <= player.interactionRange)
            {
                ScriptableQuestOffer npcQuest = npc.quests.quests[npcQuestIndex];
                if (npcQuest.completeHere)
                {
                    int index = GetIndexByName(npcQuest.quest.name);
                    if (index != -1)
                    {
                        // can complete it? (also checks inventory space for reward, if any)
                        if (CanCompleteQuestByIndex(index))
                        {
                            Quest quest = quests[index];

                            // call quest.OnCompleted to remove quest items from inventory, etc.
                            quest.OnCompleted(player);

                            // gain rewards
                            player.gold += quest.rewardGold;
                            player.itemMall.coins += quest.rewardCoins;

                            //if used Rank Addons
                            //rank.rankPoints += quest.rewardRankPoints;

                            quest.lastTimeCompleted = DateTime.Now;

                            //reward items
                            if (quest.rewardItems.Length > 0)
                            {
                                if (quest.data.chooseReward)
                                {
                                    if (rewardId > -1)
                                        player.inventory.Add(new Item(quest.rewardItems[rewardId].item), quest.rewardItems[rewardId].amount, quest.rewardItems[rewardId].item.autoBind);
                                }
                                else
                                {
                                    for (int i = 0; i < quest.rewardItems.Length; i++)
                                        player.inventory.Add(new Item(quest.rewardItems[i].item), quest.rewardItems[i].amount, quest.rewardItems[i].item.autoBind);
                                }
                            }

                            // complete quest
                            quest.completed = true;
                            quests[index] = quest;
                        }
                    }
                }
            }
        }

        [Command]
        public void CmdCompleteQuestImmediately(Quest quest, int rewardId)
        {
            // validate
            if (player.health.current > 0)
            {
                int index = GetIndexByName(quest.name);
                if (index != -1)
                {               
                    // can complete it? (also checks inventory space for reward, if any)
                    if (CanCompleteQuestExtended(quests[index]))
                    {
                        // call quest.OnCompleted to remove quest items from inventory, etc.
                        quest.OnCompleted(player);

                        // gain rewards
                        player.gold += quest.rewardGold;
                        //experience += quest.rewardExperience;

                        //if used Rank Addons
                        //rank.rankPoints += quest.rewardRankPoints;

                        player.itemMall.coins += quest.rewardCoins;
                        //skillExperience += quest.rewardExperienceSkill;
                        quest.lastTimeCompleted = DateTime.Now;
                        if (quest.rewardItems.Length > 0)
                        {
                            if (quest.data.chooseReward)
                            {
                                if (rewardId > -1)
                                player.inventory.Add(new Item(quest.rewardItems[rewardId].item), quest.rewardItems[rewardId].amount, false);
                            }
                            else
                            {
                                for (int i = 0; i < quest.rewardItems.Length; i++)
                                    player.inventory.Add(new Item(quest.rewardItems[i].item), quest.rewardItems[i].amount, false);
                            }
                        }

                        // complete quest
                        quest.completed = true;
                        quests[index] = quest;
                    }
                }
            }
        }

        [Command]
        public void CmdCancelQuestExtended(Quest RemovedQuest)
        {
            if (player.health.current > 0) quests.Remove(RemovedQuest);
        }

        //Share Quest
        [SyncVar, HideInInspector] public string shareQuestRequest = "";
        [Command]
        public void CmdShareQuestRequestSend(Quest quest)
        {
            /*for (int i = 0; i < party.members.Length; i++)
            {
                string memberName = party.members[i];
                if (memberName != name)
                {
                    if (Player.onlinePlayers.ContainsKey(memberName))
                    {
                        Player member = Player.onlinePlayers[memberName];
                        member.shareQuestRequest = quest.name;
                    }
                }
            }*/
        }
        [Command]
        public void CmdShareQuestRequestDecline()
        {
            shareQuestRequest = "";
        }

        // quests //////////////////////////////////////////////////////////////////
        public int GetIndexByName(string questName)
        {
            // (avoid Linq because it is HEAVY(!) on GC and performance)
            for (int i = 0; i < quests.Count; ++i)
                if (quests[i].name == questName)
                    return i;
            return -1;
        }

        // helper function to check if the player has completed a quest before
        public bool HasCompleted(string questName)
        {
            // (avoid Linq because it is HEAVY(!) on GC and performance)
            foreach (Quest quest in quests)
                if (quest.name == questName && quest.completed)
                    return true;
            return false;
        }

        // count the completed quests
        public int CountIncomplete()
        {
            int count = 0;
            foreach (Quest quest in quests)
                if (!quest.completed)
                    ++count;
            return count;
        }

        // helper function to check if a player has an active (not completed) quest
        public bool HasActive(string questName)
        {
            // (avoid Linq because it is HEAVY(!) on GC and performance)
            foreach (Quest quest in quests)
                if (quest.name == questName && !quest.completed)
                    return true;
            return false;
        }

        // helper function to check if the player can accept a new quest
        // note: no quest.completed check needed because we have a'not accepted yet'
        //       check
        public bool CanAcceptQuestExtended(ScriptableQuest quest)
        {
            // not too many quests yet?
            // has required level?
            // not accepted yet?
            // has finished predecessor quest (if any)?
            // guild check

            int index = GetIndexByName(quest.name);

            return
                // not too many quests yet?
                CountIncomplete() < activeQuestLimit &&

                     //quest not accepted yet or 
                     index == -1 &&
                     // has finished predecessor quest (if any)?
                     (quest.predecessor == null || HasCompleted(quest.predecessor.name))

                     // guild check
                     && ((quest.guild && player.guild.InGuild()) || !quest.guild);
        }

        [Command]
        public void CmdAcceptQuest(int npcQuestIndex)
        {
            // validate
            // use collider point(s) to also work with big entities
            if (player.health.current > 0 &&
                player.interactionExtended.target != null && player.interactionExtended.target is Npc npc &&
                0 <= npcQuestIndex && npcQuestIndex < npc.quests.quests.Length &&
                uSurvival.Utils.ClosestDistance(player.collider, npc.collider) <= player.interactionRange)
            {
                ScriptableQuestOffer npcQuest = npc.quests.quests[npcQuestIndex];
                if (npcQuest.acceptHere && CanAcceptQuestExtended(npcQuest.quest))
                {
                    int index = GetIndexByName(npc.quests.quests[npcQuestIndex].quest.name);
                    if (index != -1) quests[index] = new Quest(npcQuest.quest);
                    else quests.Add(new Quest(npcQuest.quest));
                }
            }
        }

        [Command]
        public void CmdAcceptQuest(string questname)
        {
            // validate
            // use collider point(s) to also work with big entities
            if (player.health.current > 0 && !string.IsNullOrEmpty(questname))
            {
                if (ScriptableQuest.dict.TryGetValue(uSurvival.Extensions.GetStableHashCode(questname), out ScriptableQuest questData))
                {
                    int index = GetIndexByName(questname);

                    if (index != -1) quests[index] = new Quest(questData);
                    else quests.Add(new Quest(questData));
                }
            }
        }
        [Command]
        public void CmdAcceptQuestFromPlayer(Quest quest)
        {
            // validate
            if (player.health.current > 0)
            {
                if (CanAcceptQuestExtended(quest.data))
                {
                    int index = GetIndexByName(quest.name);
                    if (index != -1) quests[index] = quest;
                    else quests.Add(new Quest(quest.data));
                }

                shareQuestRequest = "";
            }
        }

        // helper function to check if the player can complete a quest
        public bool CanCompleteQuest(string questName)
        {
            // has the quest and not completed yet?
            int index = GetIndexByName(questName);
            if (index != -1 && !quests[index].completed)
            {
                // fulfilled?
                Quest quest = quests[index];
                if (quest.IsFulfilled(player))
                {
                    // enough space for reward item (if any)?
                    return EnoughSpaceInInventory(quest);
                }
            }
            return false;
        }
        private bool CanCompleteQuestByIndex(int index)
        {
            // has the quest and not completed yet?
            if (index != -1 && !quests[index].completed)
            {
                // fulfilled?
                Quest quest = quests[index];
                if (quest.IsFulfilled(player))
                {
                    // enough space for reward item (if any)?
                    return EnoughSpaceInInventory(quest);
                }
            }
            return false;
        }
        private bool CanCompleteQuestExtended(Quest quest)
        {
            // has the quest and not completed yet?
            if (!quest.completed)
            {
                // fulfilled?
                if (quest.IsFulfilled(player))
                {
                    // enough space for reward item (if any)?
                    return EnoughSpaceInInventory(quest);
                }
            }
            return false;
        }
        private bool EnoughSpaceInInventory(Quest quest)
        {
            if (quest.rewardItems.Length == 0) return true;
            else if (quest.data.chooseReward && player.inventory.SlotsFree() >= 1) return true;
            else if (player.inventory.SlotsFree() >= quest.rewardItems.Length) return true;

            else return false;
        }

        // combat //////////////////////////////////////////////////////////////////
        [Server]
        public void OnKilledEnemy(Entity victim)
        {

            // call OnKilled in all active (not completed) quests
            for (int i = 0; i < quests.Count; ++i)
                if (!quests[i].completed)
                    quests[i].OnKilled(player, i, victim);
        }

        // ontrigger ///////////////////////////////////////////////////////////////
        [ServerCallback]
        void OnTriggerEnter(Collider col)
        {
            // quest location? then call OnLocation in active (not completed) quests
            // (we use .CompareTag to avoid .tag allocations)
            if (col.CompareTag("QuestLocation"))
            {
                for (int i = 0; i < quests.Count; ++i)
                    if (!quests[i].completed)
                    {
                        quests[i].OnLocation(player, i, col);
                        if (isLocalPlayer) UIInfoPanel.singleton.Show("Quest " + col.name + " is Completed");
                    }
            }
        }

        [ServerCallback] // called by OnTriggerEnter on client and server. use callback.
        public void QuestsOnLocation(Collider location)
        {
            // call OnLocation in all active (not completed) quests
            for (int i = 0; i < quests.Count; ++i)
                if (!quests[i].completed)
                {
                    quests[i].OnLocation(player, i, location);
                }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            player = gameObject.GetComponent<Player>();
            inventory = gameObject.GetComponent<PlayerInventory>();
            player.quests = this;
        }
#endif
    }
}