using Mirror;
using System;
using uSurvival;

namespace GFFAddons
{
    [Serializable]public struct GameMasterToolPlayer
    {
        public string name;
        public int state;

        // constructors
        public GameMasterToolPlayer(Player player)
        {
            this.name = player.name;
            this.state = player.isGameMaster == false ? 0 : 1;
        }
    }

    public class SyncListGameMasterToolPlayer : SyncList<GameMasterToolPlayer> { }
}