using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class Player
    {
        public PlayerGuild guild;
    }
}


namespace GFFAddons
{
    public partial class Npc
    {
        [Header("Components")]
        public NpcGuildManagement guildManagement;
    }
}