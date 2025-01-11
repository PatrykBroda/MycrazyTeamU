using GFFAddons;
using System;
using UnityEngine;

namespace uSurvival
{
    public partial class Database
    {
        class character_quests
        {
            public string character { get; set; }
            public string name { get; set; }
            public int progress { get; set; }
            public bool completed { get; set; }
            public DateTime lastTimeCompleted { get; set; }
            // PRIMARY KEY (character, name) is created manually.
        }

        public void Connect_Quests()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_quests>();
            connection.CreateIndex(nameof(character_quests), new[] { "character", "name" });
        }

        public void CharacterLoad_Quests(Player player)
        {
            // load quests
            foreach (character_quests row in connection.Query<character_quests>("SELECT * FROM character_quests WHERE character=?", player.name))
            {
                if (ScriptableQuest.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableQuest questData))
                {
                    Quest quest = new Quest(questData);
                    quest.progress = row.progress;
                    quest.completed = row.completed;
                    quest.lastTimeCompleted = row.lastTimeCompleted;
                    player.quests.quests.Add(quest);
                }
                else Debug.LogWarning("LoadQuests: skipped quest " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
        }

        public void CharacterSave_Quests(Player player)
        {
            // quests: remove old entries first, then add all new ones
            connection.Execute("DELETE FROM character_quests WHERE character=?", player.name);
            foreach (Quest quest in player.quests.quests)
            {
                connection.InsertOrReplace(new character_quests
                {
                    character = player.name,
                    name = quest.name,
                    progress = quest.progress,
                    completed = quest.completed,
                    lastTimeCompleted = quest.lastTimeCompleted
                });
            }
        }
    }
}




