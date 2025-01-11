using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class Database
    {
        class character_customization
        {
            public string character { get; set; }
            public string type { get; set; }
            public sbyte defaultValue { get; set; }
            public byte material { get; set; }
            //public int customValue { get; set; }
            //public float scale { get; set; }
            // PRIMARY KEY (character, type) is created manually.
        }

        class character_boughtSuits
        {
            public string account { get; set; }
            public string classname { get; set; }
            public string suitname { get; set; }

            // PRIMARY KEY (character, type) is created manually.
        }

        public void Connect_Customization()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_customization>();
            connection.CreateIndex(nameof(character_customization), new[] { "character", "type" });
            connection.CreateTable<character_boughtSuits>();
            connection.CreateIndex(nameof(character_boughtSuits), new[] { "account", "classname" });
        }

        public void CharacterSave_Customization(Player player)
        {
            // quests: remove old entries first, then add all new ones
            connection.Execute("DELETE FROM character_customization WHERE character=?", player.name);

            for (int i = 0; i < player.customization.values.Count; i++)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_customization
                {
                    character = player.name,
                    type = player.customization.values[i].type.ToString(),
                    defaultValue = player.customization.values[i].defaultValue,
                    material = player.customization.values[i].materialValue
                    //customValue = player.customization.values[i].customValue
                    //scale = player.customization.scale
                });
            }

            CharacterSave_BoughtSuits(player);
        }

        public void CharacterLoad_Customization(Player player)
        {
            for (int i = 0; i < player.customization.templates.Length; i++)
            {
                CustomizationByType temp = new CustomizationByType();
                temp.type = player.customization.templates[i].type;
                temp.defaultValue = -1;
                //temp.customValue = -1;
                player.customization.values.Add(temp);
            }

            // then load valid items and put into their slots
            // (one big query is A LOT faster than querying each slot separately)
            foreach (character_customization row in connection.Query<character_customization>("SELECT * FROM character_customization WHERE character=?", player.name))
            {
                int index = player.customization.FindTypeIndexByName(row.type);
                if (index != -1)
                {
                    CustomizationByType temp = player.customization.values[index];
                    temp.defaultValue = row.defaultValue;
                    temp.materialValue = row.material;
                    //temp.customValue = row.customValue;
                    player.customization.values[index] = temp;
                }
            }

            CharacterLoad_BoughtSuits(player);
        }

        private void CharacterSave_BoughtSuits(Player player)
        {
            // quests: remove old entries first, then add all new ones
            connection.Execute("DELETE FROM character_boughtSuits WHERE account=?", player.account);

            for (int i = 0; i < player.customization.boughtSuits.Count; i++)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_boughtSuits
                {
                    account = player.account,
                    classname = player.customization.boughtSuits[i].classname,
                    suitname = player.customization.boughtSuits[i].suitname
                });
            }
        }

        private void CharacterLoad_BoughtSuits(Player player)
        {
            // then load valid items and put into their slots
            // (one big query is A LOT faster than querying each slot separately)
            foreach (character_boughtSuits row in connection.Query<character_boughtSuits>("SELECT * FROM character_boughtSuits WHERE account=?", player.account))
            {
                BoughtSuits temp = new BoughtSuits();
                temp.classname = row.classname;
                temp.suitname = row.suitname;

                player.customization.boughtSuits.Add(temp);
            }
        }
    }
}


