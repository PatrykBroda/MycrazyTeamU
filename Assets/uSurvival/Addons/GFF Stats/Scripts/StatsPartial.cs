using GFFAddons;
using UnityEngine;
using UnityEngine.UI;

namespace GFFAddons
{
    public struct CharacterStats
    {
        public string characterName;
        public string gender;
        public string guild;
        public double lifeTime;
        public int monstersKilled;
        public int playersKill;
    }
}

namespace uSurvival
{
    public partial class Database
    {
        public void LoadTopCharacters(PlayerStatsistics player)
        {
            foreach (characters character in connection.Query<characters>("SELECT * FROM characters WHERE deleted=0 ORDER BY lifetime DESC LIMIT 10"))
            {
                CharacterStats stats = new CharacterStats();
                stats.characterName = character.name;
                stats.gender = character.classname;
                stats.guild = LoadGuildForPlayer(character.name);
                stats.lifeTime = character.lifetime;
                stats.monstersKilled = character.monstersKilled;
                stats.playersKill = character.playersKilled;
                player.players.Add(stats);
            }
        }
    }

    public partial class Player
    {
        public PlayerStatsistics statistics;
    }

    public partial class UIStatus
    {
        [Header("Killed amount Addon")]
        public Text playersKilledValue;
        public Text monstersKilledValue;
        public Text lifetimeText;

        private void Update_KilledAmount(Player player)
        {
            playersKilledValue.text = player.statistics.playersKilled.ToString();
            monstersKilledValue.text = player.statistics.monstersKilled.ToString();
            lifetimeText.text = UtilsExtended.PrettySeconds((float)player.statistics.lifetime);
        }
    }

    /*public partial class UIHud
    {
        [Header("Killed amount Addon")]
        public Text playersKilledValue;
        public Text monstersKilledValue;
        public Text lifetimeText;

        private void Update_KilledAmount(Player player)
        {
            playersKilledValue.text = player.statistics.playersKilled.ToString();
            monstersKilledValue.text = player.statistics.monstersKilled.ToString();
            lifetimeText.text = UtilsExtended.PrettySeconds((float)player.statistics.lifetime);
        }
    }*/
}


