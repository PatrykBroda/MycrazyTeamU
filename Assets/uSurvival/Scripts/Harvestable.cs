using UnityEngine;
using Mirror;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))] // Ensures that a Health component is attached
public class Harvestable : Entity
{
    [Header("Resource Settings")]
    [Tooltip("Prefab of the resource to drop (e.g., logs).")]
    public GameObject resourcePrefab;

    [Tooltip("Number of resources to drop upon death.")]
    public int resourceAmount = 1;

    [Header("Respawn Settings")]
    [Tooltip("Time in seconds before the harvestable respawns.")]
    public float respawnTime = 1200f; // 20 minutes in seconds

    // Reference to Health component
    private Health healthComponent;

    // Reference to Animator component
    private Animator animator;

    // Previous health value to detect changes
    private int previousHealth;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Harvestable: OnStartServer called.");

        healthComponent = GetComponent<Health>();

        if (healthComponent != null)
        {
            healthComponent.OnDeath += HandleDeath;
            Debug.Log("Harvestable: Subscribed to Health.OnDeath.");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no Health component attached.");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Harvestable: OnStartClient called.");

        // Get the Animator component on the client
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name} has no Animator component attached.");
        }

        // Get the Health component
        healthComponent = GetComponent<Health>();

        if (healthComponent != null)
        {
            // Initialize previousHealth with the current health
            previousHealth = healthComponent.CurrentHealth;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no Health component attached.");
        }
    }

    private void Update()
    {
        // Only proceed if we have the Health component
        if (healthComponent != null)
        {
            int currentHealth = healthComponent.CurrentHealth;

            // Check if health has decreased
            if (currentHealth < previousHealth)
            {
                Debug.Log($"[Harvestable Debug] {gameObject.name} took damage. Current Health: {currentHealth}");

                // Trigger the shake animation
                PlayShakeAnimation();
            }

            // Update previousHealth for the next frame
            previousHealth = currentHealth;
        }
    }

    /// <summary>
    /// Triggers the "Shake" animation.
    /// </summary>
    private void PlayShakeAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shake");
            Debug.Log("Harvestable: Shake animation triggered.");
        }
    }

    /// <summary>
    /// Handles the death of the harvestable entity by dropping resources and scheduling a respawn.
    /// Initiates the deactivation coroutine.
    /// </summary>
    [Server]
    private void HandleDeath()
    {
        Debug.Log($"[Harvestable Debug] {gameObject.name} has died. Current Health: {healthComponent.CurrentHealth}");
        DropResources();
        StartCoroutine(DeactivateHarvestableCoroutine()); // Start the deactivation coroutine
        StartCoroutine(RespawnCoroutine());
    }

    /// <summary>
    /// Coroutine to handle the deactivation of the harvestable entity.
    /// Sets the entire GameObject to inactive. Future enhancements like animations can be integrated here.
    /// </summary>
    /// <returns></returns>
    [Server]
    private IEnumerator DeactivateHarvestableCoroutine()
    {
        Debug.Log("Harvestable: Starting deactivation coroutine.");

        // Example: Add a delay for potential future animations/effects
        // yield return new WaitForSeconds(1f);

        gameObject.SetActive(false); // Deactivate the entire GameObject
        Debug.Log("Harvestable: Harvestable GameObject set to inactive.");

        yield break; // Ends the coroutine immediately
    }

    /// <summary>
    /// Drops the specified amount of resources at random positions around the harvestable.
    /// </summary>
    [Server]
    private void DropResources()
    {
        Debug.Log("Harvestable: DropResources called.");

        if (resourcePrefab == null)
        {
            Debug.LogError("Harvestable: Resource Prefab is not assigned.");
            return;
        }

        for (int i = 0; i < resourceAmount; i++)
        {
            // Calculate a random drop position around the harvestable within a 2-unit radius
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 2f;
            Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, 0, randomOffset.z); // Keep Y at 0 initially

            // Adjust Y position based on terrain
            RaycastHit groundHit;
            Vector3 rayOrigin = new Vector3(dropPosition.x, 100f, dropPosition.z); // Start the raycast high above
            if (Physics.Raycast(rayOrigin, Vector3.down, out groundHit, 200f, LayerMask.GetMask("Default")))
            {
                dropPosition.y = groundHit.point.y;
            }
            else
            {
                dropPosition.y = 0; // Fallback Y position
            }

            // Instantiate the resource prefab at the calculated position
            GameObject resource = Instantiate(resourcePrefab, dropPosition, Quaternion.identity);

            // Ensure the resource has a NetworkIdentity and is registered in spawnPrefabs
            if (resource.GetComponent<NetworkIdentity>() == null)
            {
                Debug.LogError($"Harvestable: Resource prefab '{resource.name}' is missing a NetworkIdentity component.");
                Destroy(resource); // Clean up to prevent orphaned objects
                continue;
            }

            // Spawn the resource across the network
            NetworkServer.Spawn(resource);
            Debug.Log($"[Resource Drop Debug] Spawned resource '{resource.name}' at {dropPosition}.");
        }

        // No longer setting isActive here since deactivation is handled via coroutine
    }

    /// <summary>
    /// Coroutine to handle the respawn of the harvestable entity after a delay.
    /// Reactivates the GameObject.
    /// </summary>
    /// <returns></returns>
    [Server]
    private IEnumerator RespawnCoroutine()
    {
        Debug.Log($"[Harvestable Debug] {gameObject.name} will respawn in {respawnTime / 60} minutes.");
        yield return new WaitForSeconds(respawnTime);

        ResetHarvestable();
    }

    /// <summary>
    /// Resets the harvestable entity's state and reactivates it.
    /// </summary>
    [Server]
    private void ResetHarvestable()
    {
        // Reactivate the harvestable GameObject
        gameObject.SetActive(true);
        Debug.Log($"Harvestable: {gameObject.name} has been reactivated.");

        // Reset health
        if (healthComponent != null)
        {
            healthComponent.ResetHealth(); // Ensure this method exists in your Health component
            Debug.Log($"[Harvestable Debug] {gameObject.name} Health reset to {healthComponent.CurrentHealth}.");

            // Update previousHealth after resetting
            previousHealth = healthComponent.CurrentHealth;
        }
        else
        {
            Debug.LogError($"Harvestable: {gameObject.name} does not have a Health component.");
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Harvestable: OnStopServer called.");

        if (healthComponent != null)
        {
            healthComponent.OnDeath -= HandleDeath;
            Debug.Log("Harvestable: Unsubscribed from Health.OnDeath.");
        }
    }
}
