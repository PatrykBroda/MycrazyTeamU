using System.Text;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "uSurvival Item/Weapon(Melee)", order = 999)]
public class MeleeWeaponItem : WeaponItem
{
    [Header("Melee Weapon Settings")]
    [Tooltip("Radius for sphere casting.")]
    public float sphereCastRadius = 0.5f; // Don't make it too big or it will hit the floor first!

    [Tooltip("Define melee-specific attack range.")]
    public float meleeAttackRange = 2f; // Renamed from attackRange

    // Usage
    public override Usability CanUseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        // Check base usability first (cooldown etc.)
        Usability baseUsable = base.CanUseHotbar(player, hotbarIndex, lookAt);
        if (baseUsable != Usability.Usable)
        {
            Debug.Log($"MeleeWeaponItem: Cannot use hotbar. Usability: {baseUsable}");
            return baseUsable;
        }

        // Not reloading?
        Usability finalUsability = player.reloading.ReloadTimeRemaining() > 0 ? Usability.Cooldown : Usability.Usable;
        Debug.Log($"MeleeWeaponItem: CanUseHotbar returned {finalUsability}");
        return finalUsability;
    }

    Entity SphereCastToLookAt(Player player, Collider collider, Vector3 lookAt, out RaycastHit hit)
    {
        // Spherecast to find out what we hit
        Vector3 origin = collider.bounds.center;
        Vector3 behindOrigin = origin - player.transform.forward * sphereCastRadius;
        Vector3 direction = (lookAt - origin).normalized;
        Debug.DrawLine(behindOrigin, lookAt, Color.red, 1);
        bool hitSomething = Utils.SphereCastWithout(
            behindOrigin,
            sphereCastRadius,
            direction,
            out hit,
            meleeAttackRange + sphereCastRadius, // Updated usage
            player.gameObject,
            player.look.raycastLayers
        );

        if (hitSomething)
        {
            // Show ray for debugging
            Debug.DrawLine(behindOrigin, hit.point, Color.cyan, 1);
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 1);

            // Attempt to get the Entity component
            Entity hitEntity = hit.transform.GetComponent<Entity>();
            if (hitEntity != null)
            {
                Debug.Log($"MeleeWeaponItem: SphereCast hit Entity '{hitEntity.gameObject.name}' at position {hit.point}.");
            }
            else
            {
                Debug.Log($"MeleeWeaponItem: SphereCast hit non-Entity object '{hit.transform.gameObject.name}' at position {hit.point}.");
            }

            return hitEntity;
        }

        Debug.Log("MeleeWeaponItem: SphereCast did not hit any objects.");
        return null;
    }

    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        // Call base function to start cooldown
        base.UseHotbar(player, hotbarIndex, lookAt);
        Debug.Log("MeleeWeaponItem: UseHotbar called.");

        // Can hit an entity?
        Entity victim = SphereCastToLookAt(player, player.collider, lookAt, out RaycastHit hit);
        if (victim != null)
        {
            // Log the hit event
            Debug.Log($"[Hit Debug] MeleeWeaponItem: Hit Entity '{victim.gameObject.name}' at position {hit.point}.");

            // Deal damage
            if (victim.health != null)
            {
                // Log current health before taking damage
                Debug.Log($"[Health Debug] {victim.gameObject.name} initial Health: {victim.health.CurrentHealth}");

                int totalDamage = player.combat.damage + damage;
                victim.health.TakeDamage(totalDamage);

                // Log the new health after taking damage
                Debug.Log($"[Health Debug] {victim.gameObject.name} took {totalDamage} damage. Current Health: {victim.health.CurrentHealth}");
            }
            else
            {
                Debug.Log($"MeleeWeaponItem: Entity '{victim.gameObject.name}' does not have a Health component.");
            }

            // Check if the victim is Harvestable
            Harvestable harvestable = victim.GetComponent<Harvestable>();
            if (harvestable != null)
            {
                Debug.Log($"MeleeWeaponItem: '{victim.gameObject.name}' is Harvestable.");
                // Optional: Implement harvesting-specific logic here
                // e.g., Update UI, play specific animations, etc.
                // Note: Audio for harvesting has been removed to fix serialization issues
            }

            // Reduce durability only if we hit something
            // (An axe doesn't lose durability if we swing it in the air)
            // (Slot might be invalid in case of hands)
            ItemSlot slot = player.hotbar.slots[hotbarIndex];
            if (slot.amount > 0)
            {
                int previousDurability = slot.item.durability;
                slot.item.durability = Mathf.Max(slot.item.durability - 1, 0);
                player.hotbar.slots[hotbarIndex] = slot;

                // Log the durability change
                Debug.Log($"[Durability Debug] Slot {hotbarIndex} durability changed from {previousDurability} to {slot.item.durability}.");

                // Optional: Log when durability reaches zero
                if (slot.item.durability == 0)
                {
                    Debug.Log($"[Durability Debug] Item in slot {hotbarIndex} has broken.");
                }
            }
            else
            {
                Debug.Log($"[Durability Debug] Ignored slot {hotbarIndex} due to zero amount.");
            }
        }
        else
        {
            // Log when no entity is hit
            Debug.Log("MeleeWeaponItem: No Entity hit.");
        }
    }

    public override void OnUsedHotbar(Player player, Vector3 lookAt)
    {
        // Find out what we hit by simulating it again to decide which sound to play
        Entity victim = SphereCastToLookAt(player, player.collider, lookAt, out RaycastHit hit);
        if (victim != null)
        {
            Harvestable harvestable = victim.GetComponent<Harvestable>();
            if (harvestable != null)
            {
                // **Removed Harvesting Audio Playback**
                /*
                if (harvestable.successfulHarvestSound)
                    player.hotbar.audioSource.PlayOneShot(harvestable.successfulHarvestSound);
                */
                Debug.Log($"MeleeWeaponItem: Harvestable '{victim.gameObject.name}' was hit.");
            }
            else
            {
                // Play the successful use sound from base class
                if (successfulUseSound)
                {
                    player.hotbar.audioSource.PlayOneShot(successfulUseSound);
                    Debug.Log("MeleeWeaponItem: Played successful use sound.");
                }

                // Log the successful weapon use
                Debug.Log($"[Use Hotbar Debug] MeleeWeaponItem: Successfully used weapon on '{victim.gameObject.name}'.");
            }

            // Additionally, log the victim's current health
            if (victim.health != null)
            {
                Debug.Log($"[Health Debug] After OnUsedHotbar: {victim.gameObject.name} Health: {victim.health.CurrentHealth}");
            }
        }
        else
        {
            // Play the failed use sound from base class
            if (failedUseSound)
            {
                player.hotbar.audioSource.PlayOneShot(failedUseSound);
                Debug.Log("MeleeWeaponItem: Played failed use sound.");
            }

            // Log the failed weapon use
            Debug.LogError("[Use Hotbar Debug] MeleeWeaponItem: Weapon use failed; no Entity hit.");
        }
    }
}
