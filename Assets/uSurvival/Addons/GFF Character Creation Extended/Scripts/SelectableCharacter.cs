// small helper script that is added to character selection previews at runtime
using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [RequireComponent(typeof(PlayerIndicator))]
    [DisallowMultipleComponent]
    public class SelectableCharacter : MonoBehaviour
    {
        // index will be set by networkmanager when creating this script
        public sbyte index = -1;

        void OnMouseDown()
        {
            // set selection index
            ((NetworkManagerSurvival)NetworkManager.singleton).selection = index;

            // show selection indicator for better feedback
            GetComponent<PlayerIndicator>().SetViaParent(transform);
            GetComponent<PlayerNetworkName>().nameOverlay.color = GetComponent<PlayerNetworkName>().colorSelected;
        }

        void Update()
        {
            // remove indicator if not selected anymore
            if (((NetworkManagerSurvival)NetworkManager.singleton).selection != index)
            {
                GetComponent<PlayerIndicator>().Clear();
                GetComponent<PlayerNetworkName>().nameOverlay.color = GetComponent<PlayerNetworkName>().colorNonSelected;
            }
        }
    }
}


