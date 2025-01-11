using Mirror;
using System;
using UnityEngine;

namespace uSurvival
{
    public partial class Player
    {
        [Header("Interaction")]
        public float interactionRange = 4;

        // some commands should have delays to avoid DDOS, too much database usage
        // or brute forcing coupons etc. we use one riskyAction timer for all.
        [SyncVar, HideInInspector] public double nextRiskyActionTime = 0; // double for long term precision


        //[SyncVar] public bool isPremium = false; // quests extended; GameControlPanel; Experience, Gold and Items Bonuses
        //[HideInInspector] public DateTime premiumEnd;
    }
}


