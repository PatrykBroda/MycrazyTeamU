using Mirror;
using TMPro;
using UnityEngine;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerNetworkName : NetworkBehaviour
    {
        [Header("Text Meshes")]
        public TextMeshPro nameOverlay;
        public Color colorSelected = Color.yellow;
        public Color colorNonSelected = Color.white;

        void Update()
        {
            // update overlays in any case, except on server-only mode
            // (also update for character selection previews etc. then)
            if (!isServerOnly) UpdateOverlays();
        }

        // overlays ////////////////////////////////////////////////////////////////
        protected void UpdateOverlays()
        {
            if (nameOverlay != null)
            {
                if (!isLocalPlayer)
                {
                    nameOverlay.text = name;
                }
            }
        }

        public override void OnSerialize(NetworkWriter writer, bool initialState)
        {
            writer.WriteString(name);
        }

        // client-side deserialization
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            name = reader.ReadString();
        }
    }
}


