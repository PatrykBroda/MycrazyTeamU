using System;
using UnityEngine;

namespace uSurvival
{
    public partial class Database
    {
        public class character_dailyRewards
        {
            public string account { get; set; }
            public int year { get; set; }
            public int month { get; set; }
            public int day { get; set; }
            public bool done { get; set; }
            public bool get { get; set; }
        }

        public void Connect_DailyRewards()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_dailyRewards>();
            connection.CreateIndex(nameof(character_dailyRewards), new[] { "account", "day" });
        }

        public void CharacterLoad_DailyRewards(Player player)
        {
            // fill all slots first
            int amountDays = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            for (int i = 0; i < amountDays; i++)
                player.dailyRewards.rewards.Add(new DailyRewardsStruct(i + 1, false, false));

            foreach (character_dailyRewards row in connection.Query<character_dailyRewards>("SELECT * FROM character_dailyRewards WHERE account=? AND year=? AND month=?", player.account, DateTime.Now.Year, DateTime.Now.Month))
            {
                if (row.day <= amountDays)
                {
                    DailyRewardsStruct reward = player.dailyRewards.rewards[row.day - 1];
                    reward.done = row.done;
                    reward.get = row.get;

                    player.dailyRewards.rewards[row.day - 1] = reward;
                }
            }
        }

        public void CharacterSave_DailyRewards(Player player)
        {
            // remove old entries first, then add all new ones
            connection.Execute("DELETE FROM character_dailyRewards WHERE account=?", player.account);

            int amountDays = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            for (int i = 0; i < player.dailyRewards.rewards.Count; ++i)
            {
                if (amountDays >= i)
                {
                    connection.InsertOrReplace(new character_dailyRewards
                    {
                        account = player.account,
                        year = DateTime.Now.Year,
                        month = DateTime.Now.Month,
                        day = player.dailyRewards.rewards[i].day,
                        done = player.dailyRewards.rewards[i].done,
                        get = player.dailyRewards.rewards[i].get
                    });
                }
            }
        }
    }
}


