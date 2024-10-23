using System.Text;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "uSurvival Item/Weapon(Melee)", order = 999)]
public class MeleeWeaponItem : WeaponItem
{
    public float sphereCastRadius = 0.5f; // Don't make it too big or it will hit the floor first!
    public float attackRange = 2f; // Define attack range as needed

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
            attackRange + sphereCastRadius,
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
            Debug.LogError($"MeleeWeaponItem: Hit Entity '{victim.gameObject.name}' at position {hit.point}.");

            // Deal damage
            if (victim.health != null)
            {
                int totalDamage = player.combat.damage + damage;
                victim.health.TakeDamage(totalDamage);
                Debug.Log($"MeleeWeaponItem: Dealt {totalDamage} damage to '{victim.gameObject.name}'. New Health: {victim.health.CurrentHealth}");
            }
            else
            {
                Debug.LogError($"MeleeWeaponItem: Entity '{victim.gameObject.name}' does not have a Health component.");
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
                slot.item.durability = Mathf.Max(slot.item.durability - 1, 0);
                player.hotbar.slots[hotbarIndex] = slot;
                Debug.Log($"MeleeWeaponItem: Reduced durability of slot {hotbarIndex} to {slot.item.durability}.");
            }
            else
            {
                Debug.Log($"MeleeWeaponItem: Ignored slot {hotbarIndex} due to zero amount.");
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
                Debug.LogError($"MeleeWeaponItem: Successfully used weapon on '{victim.gameObject.name}'.");
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
            Debug.LogError("MeleeWeaponItem: Weapon use failed; no Entity hit.");
        }
    }
}
