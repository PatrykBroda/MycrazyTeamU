using GFFAddons;
using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class Player
    {
        [Header("PvP")]
        //public LinearFloat buffTime = new LinearFloat { baseValue = 60 };

        [HideInInspector] public SafeZone currentSafeZoneSource;


        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerEnter(Collider col)
        {
            // heat source?
            SafeZone safeZoneSource = col.GetComponent<SafeZone>();
            if (safeZoneSource != null)
            {
                // none yet?
                if (currentSafeZoneSource == null)
                {
                    currentSafeZoneSource = safeZoneSource;
                    collider.enabled = false;
                }
                // otherwise keep closest one
                else if (currentSafeZoneSource != safeZoneSource) // different one? otherwise don't bother with calculations
                {
                    float oldDistance = Vector3.Distance(transform.position, currentSafeZoneSource.transform.position);
                    float newDistance = Vector3.Distance(transform.position, safeZoneSource.transform.position);
                    if (newDistance < oldDistance)
                    {
                        currentSafeZoneSource = safeZoneSource;
                        collider.enabled = false;
                    }
                }
            }

            SafeZoneWithSecurity safeZoneWithSecuritySource = col.GetComponent<SafeZoneWithSecurity>();
            if (safeZoneWithSecuritySource != null)
            {
                // none yet?
                if (currentSafeZoneWithSecuritySource == null)
                {
                    currentSafeZoneWithSecuritySource = safeZoneWithSecuritySource;
                }
                // otherwise keep closest one
                else if (currentSafeZoneWithSecuritySource != safeZoneWithSecuritySource) // different one? otherwise don't bother with calculations
                {
                    float oldDistance = Vector3.Distance(transform.position, currentSafeZoneWithSecuritySource.transform.position);
                    float newDistance = Vector3.Distance(transform.position, safeZoneWithSecuritySource.transform.position);
                    if (newDistance < oldDistance)
                    {
                        currentSafeZoneWithSecuritySource = safeZoneWithSecuritySource;
                    }
                }
            }
        }

        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerExit(Collider col)
        {
            SafeZone safeZoneSource = col.GetComponent<SafeZone>();
            if (safeZoneSource != null)
            {
                if (currentSafeZoneSource != null && currentSafeZoneSource.transform == col.transform)
                {
                    currentSafeZoneSource = null;
                    collider.enabled = true;
                }           
            }

            SafeZoneWithSecurity safeZoneWithSecuritySource = col.GetComponent<SafeZoneWithSecurity>();
            if (safeZoneWithSecuritySource != null)
            {
                if (currentSafeZoneWithSecuritySource != null && currentSafeZoneWithSecuritySource.transform == col.transform)
                {
                    currentSafeZoneWithSecuritySource = null;
                }
            }
        }
    }
}