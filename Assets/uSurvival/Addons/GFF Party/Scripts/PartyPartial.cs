using GFFAddons;
using Mirror;

namespace uSurvival
{
    public partial class Entity
    {
        public float VisRange() => ((SpatialHashingInterestManagement)NetworkServer.aoi).visRange;
    }

    public partial class Player
    {
        public PlayerParty party;
    }
}