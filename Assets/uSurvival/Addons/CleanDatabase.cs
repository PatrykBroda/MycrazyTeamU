using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uSurvival
{
    public partial class Database
    {
        public void RemoveDeletedPlayers()
        {
            List<characters> players = new List<characters>();
            foreach (characters character in connection.Query<characters>("SELECT * FROM characters WHERE deleted=1"))
                players.Add(character);

            Debug.Log(players.Count + " Players will be removed");

            for (int i = 0; i < players.Count; i++)
            {
                connection.Execute("DELETE FROM character_inventory WHERE character=?", players[i].name);
                connection.Execute("DELETE FROM character_equipment WHERE character=?", players[i].name);
                connection.Execute("DELETE FROM character_customization WHERE character=?", players[i].name);
                //connection.Execute("DELETE FROM character_combatSkills WHERE character=?", players[i].name);
                connection.Execute("DELETE FROM character_groups WHERE character=?", players[i].name);
                connection.Execute("DELETE FROM character_friends WHERE character=?", players[i].name);


                //connection.Execute("DELETE FROM character_dailyRewards WHERE character=?", players[i].name);
                //connection.Execute("DELETE FROM character_boughtSuits WHERE account=?", players[i].account);
                //connection.Execute("DELETE FROM character_storage WHERE owner=?", players[i].account);

                connection.Execute("DELETE FROM characters WHERE name=?", players[i].name);
            }
        }
    }
}


