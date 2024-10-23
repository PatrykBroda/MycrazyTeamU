using UnityEngine;
using Mirror;
using System.Collections;

[DisallowMultipleComponent]
public class Harvestable : Entity
{
    [Header("Resource Settings")]
    [Tooltip("Prefab of the resource to drop (e.g., logs).")]
    public GameObject resourcePrefab;

    [Tooltip("Number of resources to drop upon death.")]
    public int resourceAmount = 1;

    [Header("Respawn Settings")]
    [Tooltip("Time in seconds before the harvestable respawns.")]
    public float respawnTime = 60f; // Time in seconds to respawn

    // Removed AudioClip and ParticleSystem fields as per previous steps

    // No duplicate 'health' field. Using inherited 'health' from Entity.

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Harvestable: OnStartServer called.");

        if (health != null)
        {
            health.OnDeath += HandleDeath;
            Debug.Log("Harvestable: Subscribed to Health.OnDeath.");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no Health component attached.");
        }
    }

    /// <summary>
    /// Handles the death of the harvestable entity by dropping resources and scheduling a respawn.
    /// </summary>
    [Server]
    private void HandleDeath()
    {
        Debug.Log("Harvestable: HandleDeath called.");
        DropResources();
        StartCoroutine(RespawnCoroutine());
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
            // Calculate a random drop position around the harvestable
            Vector3 dropPosition = transform.position + UnityEngine.Random.insideUnitSphere * 2f;
            dropPosition.y = 0; // Adjust Y position as needed for your game

            // Instantiate and spawn the resource on the network
            GameObject resource = Instantiate(resourcePrefab, dropPosition, Quaternion.identity);
            NetworkServer.Spawn(resource);
            Debug.Log($"Harvestable: Spawned resource '{resource.gameObject.name}' at {dropPosition}.");
        }

        // Destroy the harvestable entity on the server
        NetworkServer.Destroy(gameObject);
        Debug.Log("Harvestable: Destroyed harvestable GameObject.");
    }

    /// <summary>
    /// Coroutine to handle the respawn of the harvestable entity after a delay.
    /// </summary>
    /// <returns></returns>
    [Server]
    private IEnumerator RespawnCoroutine()
    {
        Debug.Log($"Harvestable: RespawnCoroutine started. Will respawn in {respawnTime} seconds.");
        yield return new WaitForSeconds(respawnTime);

        // Reinstantiate the harvestable object at the original position and rotation
        GameObject respawnedObject = Instantiate(gameObject, transform.position, transform.rotation);
        NetworkServer.Spawn(respawnedObject);
        Debug.Log("Harvestable: Respawned harvestable GameObject.");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Harvestable: OnStopServer called.");

        if (health != null)
        {
            health.OnDeath -= HandleDeath;
            Debug.Log("Harvestable: Unsubscribed from Health.OnDeath.");
        }
    }

    /// <summary>
    /// Override to handle actions when the object is instantiated on clients.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Harvestable: OnStartClient called.");
        // Optional: Initialize client-side visual components
    }
}
