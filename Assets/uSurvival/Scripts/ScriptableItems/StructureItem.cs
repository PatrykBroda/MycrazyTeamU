using System;
using UnityEngine;
using Mirror;

// Objects need position offsets for some grid sizes.
// (e.g. if grid = 1 and we want to build a thin palette, it should not be in
//  the center of a unit cube, but on the sides)
[Serializable]
public struct CustomRotation
{
    public Vector3 positionOffset;
    public Vector3 rotation;
}

[CreateAssetMenu(menuName = "uSurvival Item/Structure", order = 999)]
public class StructureItem : UsableItem
{
    // Shown while deciding where to build
    [Header("Structure")]
    public GameObject structurePrefab;

    // Show the preview 'n' units away from player.
    // -> For example, a huge building needs to be shown at least size/2 away from the player
    public float previewDistance = 2;

    // Resolution 1: 1, 2, 3, 4, ...
    // Resolution 2: 0.5, 1, 1.5, 2, ...
    // Note: Modify availableRotations[] position offsets to fit grid if needed
    [Range(1, 10)] public int gridResolution = 1;

    // Rotation key switches through available rotations
    public CustomRotation[] availableRotations = { new CustomRotation() };

    // Can build checks
    [Range(0, 1)] public float buildToleranceCollision = 0.1f;
    [Range(0, 1)] public float buildToleranceAir = 0.1f;

    // Can't use it from inventory. Need to aim where to build it
    public override Usability CanUseInventory(Player player, int inventoryIndex) { return Usability.Never; }

    public override Usability CanUseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        // Check base usability first (cooldown, etc.)
        Usability baseUsable = base.CanUseHotbar(player, hotbarIndex, lookAt);
        if (baseUsable != Usability.Usable)
            return baseUsable;

        // Calculate look direction in a way that works on clients and server (via lookAt)
        Vector3 lookDirection = (lookAt - player.look.headPosition).normalized;

        // Calculate bounds based on structurePrefab + position + rotation
        Vector3 position = player.construction.CalculatePreviewPosition(this, player.look.headPosition, lookDirection);
        Quaternion rotation = player.construction.CalculatePreviewRotation(); // Fixed: Removed 'this'

        // We need the structure prefab's bounds, but rotated and positioned to where we want to build.
        Vector3 prefabPosition = structurePrefab.transform.position;
        Quaternion prefabRotation = structurePrefab.transform.rotation;
        structurePrefab.transform.position = position;
        structurePrefab.transform.rotation = rotation;
        Bounds bounds = structurePrefab.GetComponentInChildren<Renderer>().bounds;
        structurePrefab.transform.position = prefabPosition;
        structurePrefab.transform.rotation = prefabRotation;

        return CanBuildThere(player.look.headPosition, bounds, player.look.raycastLayers)
               ? Usability.Usable
               : Usability.Empty; // For empty sound. Better than 'Never'.
    }

    // [Server] Use logic
    public override void UseInventory(Player player, int inventoryIndex) { }

    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        // Call base function to start cooldown
        base.UseHotbar(player, hotbarIndex, lookAt);

        // Get position and rotation from Construction component
        // Calculate look direction in a way that works on clients and server (via lookAt)
        Vector3 lookDirection = (lookAt - player.look.headPosition).normalized;
        Vector3 position = player.construction.CalculatePreviewPosition(this, player.look.headPosition, lookDirection);
        Quaternion rotation = player.construction.CalculatePreviewRotation(); // Fixed: Removed 'this'

        // Spawn it into the world
        GameObject go = Instantiate(structurePrefab, position, rotation);
        go.name = structurePrefab.name; // Avoid "(Clone)". Important for saving..
        NetworkServer.Spawn(go);

        // Decrease amount
        ItemSlot slot = player.hotbar.slots[hotbarIndex];
        slot.DecreaseAmount(1);
        player.hotbar.slots[hotbarIndex] = slot;
    }

    // Can we build the structure at this position with this rotation?
    // -> Having CanBuildAt in here instead of in Construction script allows for
    //    custom structures like windows that can only be built between walls
    public virtual bool CanBuildThere(Vector3 headPosition, Bounds bounds, LayerMask raycastLayers)
    {
        // Nothing at this position yet?
        // => We check 90% of size so things can at least barely touch each other
        if (Physics.CheckBox(bounds.center, bounds.extents * (1 - buildToleranceCollision), Quaternion.identity, raycastLayers))
            return false;

        // Not floating in air?
        // => Needs to touch anything else if 110% of collider
        if (!Physics.CheckBox(bounds.center, bounds.extents * (1 + buildToleranceAir), Quaternion.identity, raycastLayers))
            return false;

        // Linecast to make sure that nothing is between us and the build preview
        return !Physics.Linecast(headPosition, bounds.center, raycastLayers);
    }

    protected override void OnValidate()
    {
        // Call base function
        base.OnValidate();

        // Need at least one available rotation
        if (availableRotations.Length == 0)
            availableRotations = new CustomRotation[] { new CustomRotation() };
    }
}
