using GFFAddons;
using SQLite; // from https://github.com/praeclarum/sqlite-net

namespace uSurvival
{
    public partial class Database
    {
        class character_craftingSkills
        {
            [PrimaryKey] // important for performance: O(log n) instead of O(n)
            public string character { get; set; }
            public string lvs { get; set; }
            public string exp { get; set; }
        }

        class character_craftingStudiedRecipes
        {
            public string character { get; set; }
            public string recipe { get; set; }
        }

        public void Connect_CraftingExtended()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_craftingSkills>();

            connection.CreateTable<character_craftingStudiedRecipes>();
            connection.CreateIndex(nameof(character_craftingStudiedRecipes), new[] { "character", "recipe" });
        }

        public void CharacterLoad_CraftingSkills(Player player)
        {
            character_craftingSkills row = connection.FindWithQuery<character_craftingSkills>("Select * FROM character_craftingSkills WHERE character=?", player.name);

            // load skills based on skill templates
            foreach (ScriptableCraftingSkill skillData in player.craftingExtended.skillTemplates)
                player.craftingExtended.skills.Add(new CraftingSkill(skillData));

            if (row != null)
            {
                string lvl = row.lvs;
                string exp = row.exp;
                //We are looping through all instances of the letter in the given string
                int i = 0;
                while (lvl.IndexOf(";") != -1)
                {
                    CraftingSkill temp = player.craftingExtended.skills[i];
                    temp.level = int.Parse(lvl.Substring(0, lvl.IndexOf(";")));
                    temp.exp = uint.Parse(exp.Substring(0, exp.IndexOf(";")));
                    player.craftingExtended.skills[i] = temp;

                    lvl = lvl.Remove(0, lvl.IndexOf(";") + 1);
                    exp = exp.Remove(0, exp.IndexOf(";") + 1);
                    i++;
                }
            }
        }
        public void CharacterSave_CraftingSkills(Player player)
        {
            connection.InsertOrReplace(new character_craftingSkills
            {
                character = player.name,
                lvs = SaveSkillsLv(player),
                exp = SaveSkillsExp(player)
            });
        }

        string SaveSkillsLv(Player player)
        {
            string temp = "";
            for (int i = 0; i < player.craftingExtended.skills.Count; i++)
                temp += player.craftingExtended.skills[i].level + ";";

            return temp;
        }
        string SaveSkillsExp(Player player)
        {
            string temp = "";
            for (int i = 0; i < player.craftingExtended.skills.Count; i++)
                temp += player.craftingExtended.skills[i].exp + ";";

            return temp;
        }

        public void CharacterLoad_CraftingRecipes(Player player)
        {
            if (player.craftingExtended.useLearnedRecipes)
            {
                // (one big query is A LOT faster than querying each slot separately)
                foreach (character_craftingStudiedRecipes row in connection.Query<character_craftingStudiedRecipes>("SELECT * FROM character_craftingStudiedRecipes WHERE character=?", player.name))
                {
                    player.craftingExtended.learnedCraftingRecipes.Add(row.recipe);
                }
            }
        }
        public void CharacterSave_CraftingRecipes(Player player)
        {
            if (player.craftingExtended.useLearnedRecipes)
            {
                for (int i = 0; i < player.craftingExtended.learnedCraftingRecipes.Count; ++i)
                {
                    // note: .Insert causes a 'Constraint' exception. use Replace.
                    connection.InsertOrReplace(new character_craftingStudiedRecipes
                    {
                        character = player.name,
                        recipe = player.craftingExtended.learnedCraftingRecipes[i]
                    });
                }
            }
        }
    }
}

