using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using uSurvival;

namespace GFFAddons
{
    [Serializable]
    public class Rewards
    {
        [HideInInspector] public string name;
        public uint gold;
        public uint coins;
        public ScriptableItemAndAmount[] items;
    }

    [DisallowMultipleComponent]
    public class PlayerDailyRewards : NetworkBehaviour
    {
        [SerializeField] private Rewards[] rewardsTemplates = new Rewards[31];
        [SerializeField] private float timeSpentInTheGame = 30f;
        [SerializeField] private bool autoOpenRewardsPanel = true;
        [SerializeField] private bool autoAddRewards = false;

        [Header("Components")]
        public Player player;

        public readonly SyncListDailyRewards rewards = new SyncListDailyRewards();

        public override void OnStartServer()
        {
            //has a reward been received for this day ?
            //find this day in rewards list
            for (byte i = 0; i < rewards.Count; i++)
            {
                if (rewards[i].day == DateTime.Now.Day)
                {
                    if (!rewards[i].done)
                    {
                        //to receive a reward need to spend time in the game ?
                        StartCoroutine(DailyRewardComplete(i, timeSpentInTheGame));
                    }
                    else if (!rewards[i].get) StartCoroutine(DailyRewardComplete(i, 2));

                    break;
                }
            }
        }

        private IEnumerator DailyRewardComplete(byte rewardIndex, float awaitTime)
        {
            yield return new WaitForSeconds(awaitTime);
            DailyRewardsStruct reward = rewards[rewardIndex];
            reward.done = true;
            rewards[rewardIndex] = reward;
            if (autoOpenRewardsPanel) RpcDailyRewardComplete(rewardIndex);
        }

        public Rewards GetRewardsByDay(int day)
        {
            if (day <= rewardsTemplates.Length)
                return rewardsTemplates[day];
            else return null;
        }

        [ClientRpc]
        private void RpcDailyRewardComplete(int selectedDay)
        {
            if (player.isLocalPlayer && player.isClient)
                UIDailyRewards.singleton.Show(selectedDay);
        }

        [Command]
        public void CmdAddReward(int selectedDay)
        {
            if (selectedDay != -1 && rewards[selectedDay].done && rewards[selectedDay].get == false)
            {
                Rewards reward = rewardsTemplates[selectedDay];

                //if inventory is ok
                if (player.inventory.SlotsFree() >= reward.items.Length)
                {
                    //add gold
                    player.gold += reward.gold;

                    //add coins
                    player.itemMall.coins += reward.coins;

                    //add items
                    for (int x = 0; x < reward.items.Length; ++x)
                    {
                        if (reward.items[x].item != null)
                        {
                            player.inventory.Add(new Item(reward.items[x].item), reward.items[x].amount, reward.items[x].item.autoBind);
                        }
                    }

                    //update state
                    DailyRewardsStruct go = rewards[selectedDay];
                    go.get = true;
                    rewards[selectedDay] = go;
                }
                else TargetSendMessage("Inventory is full");
            }
        }

        [TargetRpc] // only send to one client
        private void TargetSendMessage(string message)
        {
            UIDailyRewards.singleton.ShowInfoMessage(message);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (syncInterval == 0)
            {
                syncInterval = 0.1f;
            }

            player = gameObject.GetComponent<Player>();
            player.dailyRewards = this;

            //add events to database
            Database database = FindAnyObjectByType<Database>();
            if (database)
            {
                UnityAction unityAction = new UnityAction(database.Connect_DailyRewards);
                EventsPartial.AddListenerOnceOnConnected(database.onConnected, unityAction, database);

                UnityAction<Player> load = new UnityAction<Player>(database.CharacterLoad_DailyRewards);
                EventsPartial.AddListenerOnceCharacterLoad(database.onCharacterLoad, load, database);

                UnityAction<Player> save = new UnityAction<Player>(database.CharacterSave_DailyRewards);
                EventsPartial.AddListenerOnceCharacterSave(database.onCharacterSave, save, database);
            }

            for (int i = 0; i < rewardsTemplates.Length; i++)
            {
                rewardsTemplates[i].name = "Day " + (i + 1);

                for (int x = 0; x < rewardsTemplates[i].items.Length; x++)
                {
                    if (rewardsTemplates[i].items[x].amount == 0) rewardsTemplates[i].items[x].amount = 1;

                    if (rewardsTemplates[i].items[x].item != null) rewardsTemplates[i].items[x].name = rewardsTemplates[i].items[x].item.name;
                    else rewardsTemplates[i].items[x].name = "null";
                }
            }
        }
#endif
    }
}