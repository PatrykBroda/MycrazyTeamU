using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class Player
    {
        public PlayerNameOfDistricts nameOfDistricts;
    }
}

namespace GFFAddons
{
    public class PlayerNameOfDistricts : MonoBehaviour
    {
        [HideInInspector] public NameOfDistricts currentDistrictSource;

        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerEnter(Collider col)
        {
            // heat source?
            NameOfDistricts districtSource = col.GetComponent<NameOfDistricts>();
            if (districtSource != null)
            {
                // none yet?
                if (currentDistrictSource == null)
                {
                    currentDistrictSource = districtSource;
                }
                // otherwise keep closest one
                else if (currentDistrictSource != districtSource) // different one? otherwise don't bother with calculations
                {
                    float oldDistance = Vector3.Distance(transform.position, currentDistrictSource.transform.position);
                    float newDistance = Vector3.Distance(transform.position, districtSource.transform.position);
                    if (newDistance < oldDistance)
                        currentDistrictSource = districtSource;
                }
            }
        }

        // [Client] & [Server] so we don't need a SyncVar
        public void OnTriggerExit(Collider col)
        {
            NameOfDistricts districtSource = col.GetComponent<NameOfDistricts>();
            if (districtSource != null)
            {
                if (currentDistrictSource != null && currentDistrictSource.transform == col.transform)
                    currentDistrictSource = null;
            }
        }
    }
}