using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public class PlayerRestore : NetworkBehaviour
    {
        [Header("Components")]
        public Health health;
        public PlayerChat chat;

        public RestoreZone currentRestoreZoneSource;
        [SyncVar] public Vector3 savePosition;

        public override void OnStartServer()
        {
            InvokeRepeating(nameof(RestoreZone), 1, 1);
        }

        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerEnter(Collider col)
        {
            // heat source?
            RestoreZone restoreZoneSource = col.GetComponent<RestoreZone>();
            if (restoreZoneSource != null)
            {
                // none yet?
                if (currentRestoreZoneSource == null)
                {
                    currentRestoreZoneSource = restoreZoneSource;
                }
                // otherwise keep closest one
                else if (currentRestoreZoneSource != restoreZoneSource) // different one? otherwise don't bother with calculations
                {
                    float oldDistance = Vector3.Distance(transform.position, currentRestoreZoneSource.transform.position);
                    float newDistance = Vector3.Distance(transform.position, restoreZoneSource.transform.position);
                    if (newDistance < oldDistance)
                        currentRestoreZoneSource = restoreZoneSource;
                }
            }
        }

        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerExit(Collider col)
        {
            // check if trigger first to avoid GetComponent tests for environment
            if (col.GetComponent<RestoreZone>())
            {
                currentRestoreZoneSource = null;
            }
        }

        public void RestoreZone()
        {
            if (currentRestoreZoneSource != null && health.current > 0 && health.current < health.max)
            {
                Debug.Log("restore zone");
                health.current += currentRestoreZoneSource.restoreHealth;
                //chat.TargetMsgSafeZone("Востановление Здоровья +" + currentRestoreZoneSource.restoreHealth);
            }
        }
    }
}


