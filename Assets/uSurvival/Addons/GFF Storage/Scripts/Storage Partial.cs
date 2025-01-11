using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        [Header("GFF Storage Addon")]
        public bool canPutInTheStorage = true;
    }

    public partial class Player
    {
        [Header("GFF Storage Addon")]
        public PlayerStorage storage;
    }
}

namespace GFFAddons
{
    public partial class Npc
    {
        [Header("GFF Storage Addon")]
        public NpcStorage storage;
    }
}
