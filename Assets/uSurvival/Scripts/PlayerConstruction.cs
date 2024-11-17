// This component handles the construction preview while a structure item is
// selected on the hotbar
using UnityEngine;
using Mirror;

public class PlayerConstruction : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public PlayerHotbar hotbar;
    public PlayerLook look;

    [Header("Configuration")]
    public KeyCode rotationKey = KeyCode.R;
    public Color canBuildColor = Color.cyan;
    public Color cantBuildColor = Color.red;

    // current preview (on client)
    // -> no need to sync it to server. server can estimate it via:
    //    - position from look.raycast direction
    //    - rotation from rotationIndex
    //    - bounds from structure.previewPrefab
    [HideInInspector] public GameObject preview;

    // rotation index needs to be known on the server to decide final build
    // position / rotation
    [SyncVar] int rotationIndex;

    // Helper function to get the current structure in hands (if any)
    public StructureItem GetCurrentStructure()
    {
        UsableItem itemData = hotbar.GetCurrentUsableItemOrHands();
        return itemData is StructureItem ? (StructureItem)itemData : null;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Selected a structure in hotbar right now?
        StructureItem structure = GetCurrentStructure();
        if (structure != null)
        {
            // Destroy preview if not matching anymore
            if (preview != null && preview.name != structure.structurePrefab.name)
                Destroy(preview);

            // Load preview if not loaded yet
            if (preview == null || preview.name != structure.structurePrefab.name)
            {
                // Instantiate
                preview = Instantiate(structure.structurePrefab,
                                      CalculatePreviewPosition(structure, look.headPosition, look.lookDirectionRaycasted),
                                      CalculatePreviewRotation());

                // Avoid "(Clone)"
                preview.name = structure.structurePrefab.name;

                // Remove all script logic. It's only used as a preview.
                // (Except NetworkIdentity, which would throw an error)
                Behaviour[] behaviours = preview.GetComponentsInChildren<Behaviour>();
                for (int i = behaviours.Length - 1; i >= 0; --i)
                    if (!(behaviours[i] is NetworkIdentity))
                        Destroy(behaviours[i]);

                // Remove all colliders. It's only used as a preview.
                foreach (Collider co in preview.GetComponentsInChildren<Collider>())
                    Destroy(co);
            }

            // Set position in front of player
            preview.transform.position = CalculatePreviewPosition(structure, look.headPosition, look.lookDirectionRaycasted);

            // Set rotation
            preview.transform.rotation = CalculatePreviewRotation();

            // Rotate if R key pressed
            if (Input.GetKeyDown(rotationKey))
            {
                int newIndex = (rotationIndex + 1) % 4; // Only 4 possible rotations (0, 90, 180, 270 degrees)
                CmdSetRotationIndex(newIndex);
            }

            // Set color depending on if we can build there or not
            Bounds bounds = preview.GetComponentInChildren<Renderer>().bounds;
            bool canBuild = structure.CanBuildThere(look.headPosition, bounds, look.raycastLayers);
            foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>())
                renderer.material.color = canBuild ? canBuildColor : cantBuildColor;
        }
        // No more structure selected. Destroy preview (if any)
        else if (preview != null) Destroy(preview);
    }

    [Command]
    void CmdSetRotationIndex(int index)
    {
        rotationIndex = index;
    }

    static float RoundToGrid(float value, int resolution)
    {
        return Mathf.Round(value * resolution) / resolution;
    }

    public Vector3 CalculatePreviewPosition(StructureItem structure, Vector3 headPosition, Vector3 lookDirection)
    {
        Vector3 inFront = headPosition + lookDirection * structure.previewDistance;

        // Snap to grid so we have some kind of order when building
        inFront.x = RoundToGrid(inFront.x, structure.gridResolution);
        inFront.y = RoundToGrid(inFront.y, structure.gridResolution);
        inFront.z = RoundToGrid(inFront.z, structure.gridResolution);

        return inFront + structure.availableRotations[rotationIndex % structure.availableRotations.Length].positionOffset;
    }

    public Quaternion CalculatePreviewRotation()
    {
        // Calculate rotation only on the Y-axis
        float rotationY = rotationIndex * 90f;
        return Quaternion.Euler(0f, rotationY, 0f);
    }

    void OnDrawGizmos()
    {
        if (preview != null)
        {
            StructureItem structure = GetCurrentStructure();
            Bounds bounds = preview.GetComponentInChildren<Renderer>().bounds;

            // Regular bounds
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Bounds - build tolerance (for collision check)
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(bounds.center, bounds.size * (1 - structure.buildToleranceCollision));

            // Bounds + build tolerance (for air check)
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size * (1 + structure.buildToleranceAir));
        }
    }
}
