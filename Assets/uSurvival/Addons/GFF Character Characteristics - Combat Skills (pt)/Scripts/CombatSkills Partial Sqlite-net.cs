using GFFAddons;
using SQLite; // from https://github.com/praeclarum/sqlite-net

namespace uSurvival
{
    public partial class Database
    {
        public class character_combatSkills
        {
            [PrimaryKey] // important for performance: O(log n) instead of O(n)
            public string character { get; set; }
            public string lvs { get; set; }
            public string exp { get; set; }
        }

        public void Connect_CombatSkills()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_combatSkills>();
        }

        private void CharacterLoad_CombatSkills(Player player)
        {
            character_combatSkills row = connection.FindWithQuery<character_combatSkills>("Select * FROM character_combatSkills WHERE character=?", player.name);

            // load skills based on skill templates
            foreach (CombatSkillItem skillData in player.combatSkills.skillTemplates)
                player.combatSkills.skills.Add(new CombatSkill(skillData));

            if (row != null)
            {
                string lvl = row.lvs;
                string exp = row.exp;
                //We are looping through all instances of the letter in the given string
                int i = 0;
                while (lvl.IndexOf(";") != -1)
                {
                    CombatSkill temp = player.combatSkills.skills[i];
                    temp.level = byte.Parse(lvl.Substring(0, lvl.IndexOf(";")));
                    temp.exp = long.Parse(exp.Substring(0, exp.IndexOf(";")));
                    player.combatSkills.skills[i] = temp;

                    lvl = lvl.Remove(0, lvl.IndexOf(";") + 1);
                    exp = exp.Remove(0, exp.IndexOf(";") + 1);
                    i++;
                }
            }
        }

        private void Load_CombatSkills(Player player, string lvl, string exp)
        {
            // load skills based on skill templates
            foreach (CombatSkillItem skillData in player.combatSkills.skillTemplates)
                player.combatSkills.skills.Add(new CombatSkill(skillData));

            if (string.IsNullOrEmpty(lvl) == false && string.IsNullOrEmpty(exp) == false)
            {
                //string lvl = row.lvs;
                //string exp = row.exp;
                //We are looping through all instances of the letter in the given string
                int i = 0;
                while (lvl.IndexOf(";") != -1)
                {
                    CombatSkill temp = player.combatSkills.skills[i];
                    temp.level = byte.Parse(lvl.Substring(0, lvl.IndexOf(";")));
                    temp.exp = long.Parse(exp.Substring(0, exp.IndexOf(";")));
                    player.combatSkills.skills[i] = temp;

                    lvl = lvl.Remove(0, lvl.IndexOf(";") + 1);
                    exp = exp.Remove(0, exp.IndexOf(";") + 1);
                    i++;
                }
            } 
        }

        private void CharacterSave_CombatSkills(Player player)
        {
            connection.Execute("DELETE FROM character_combatSkills WHERE character=?", player.name);

            connection.InsertOrReplace(new character_combatSkills
            {
                character = player.name,

                lvs = SaveCombatSkillLv(player),
                exp = SaveCombatSkillExp(player)
            });
        }

        private string SaveCombatSkillLv(Player player)
        {
            string temp = "";
            for (int i = 0; i < player.combatSkills.skills.Count; i++)
                temp += player.combatSkills.skills[i].level + ";";

            return temp;
        }
        private string SaveCombatSkillExp(Player player)
        {
            string temp = "";
            for (int i = 0; i < player.combatSkills.skills.Count; i++)
                temp += player.combatSkills.skills[i].exp + ";";

            return temp;
        }
    }
}
