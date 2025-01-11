using GFFAddons;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using uSurvival;

namespace uSurvival
{
    public partial class Player
    {
        public PlayerQuests quests;
    }

    public partial class Combat
    {
        [Header("Events")]
        public UnityEventEntity onDamageDealtTo;
        public UnityEventEntity onKilledEnemy;
    }
}

namespace GFFAddons
{
    [Serializable] public class UnityEventEntity : UnityEvent<Entity> { }

    public partial class Npc
    {
        public NpcQuests quests;
    }
}