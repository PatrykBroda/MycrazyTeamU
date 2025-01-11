using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerStatsistics : NetworkBehaviour
    {
        [Header("Settings")]
        public float statsUpdateTime = 60;

        [HideInInspector] public readonly SyncList<CharacterStats> players = new SyncList<CharacterStats>();

        [SyncVar] public ushort playersKilled = 0;
        [SyncVar] public ushort monstersKilled = 0;
        [SyncVar] public double lifetime = 0;

        private void IncreaseLifetime()
        {
            lifetime += 1;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            InvokeRepeating(nameof(LoadStatistics), 1, statsUpdateTime);
            InvokeRepeating(nameof(IncreaseLifetime), 1, 1);
        }

        [Server] private void LoadStatistics()
        {
            players.Clear();
            Database.singleton.LoadTopCharacters(this);
        }

        [Server]public void OnDeath()
        {
            lifetime = 0;
            monstersKilled = 0;
            playersKilled = 0;
        }
    }
}


