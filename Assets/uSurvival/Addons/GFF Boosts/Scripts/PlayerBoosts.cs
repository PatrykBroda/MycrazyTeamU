using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerBoosts : NetworkBehaviour, IHealthBonus
    {
        // recovery tick rate
        // (1/s for health, but might need to be less for something like temperature
        //  where we don't want to reduce it by -1C per second, etc.)
        public int recoveryTickRate = 1;

        [SyncVar] public byte boost = 0;

        public override void OnStartServer()
        {
            // recovery every 'TickRate'
            InvokeRepeating(nameof(Recover), recoveryTickRate, recoveryTickRate);
        }

        // recover once a second
        [Server]
        public void Recover()
        {
            if (boost > 0)
            {
                boost -= 1;
            }
        }

        public short HealthBonus(short baseHealth)
        {
            return 0;
        }

        public short HealthRecoveryBonus()
        {
            if (boost > 0) return 4;
            else return 0;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            GetComponent<Player>().boosts = this;
        }
#endif
    }
}