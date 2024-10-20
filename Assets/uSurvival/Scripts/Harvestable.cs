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

    private Health health;

    public override void OnStartServer()
    {
        base.OnStartServer();
        health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath += HandleDeath;
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
        DropResources();
        StartCoroutine(RespawnCoroutine());
    }

    /// <summary>
    /// Drops the specified amount of resources at random positions around the harvestable.
    /// </summary>
    [Server]
    private void DropResources()
    {
        if (resourcePrefab == null)
        {
            Debug.LogError("Resource Prefab is not assigned in Harvestable.");
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
        }

        // Destroy the harvestable entity on the server
        NetworkServer.Destroy(gameObject);
    }

    /// <summary>
    /// Coroutine to handle the respawn of the harvestable entity after a delay.
    /// </summary>
    /// <returns></returns>
    [Server]
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);

        // Reinstantiate the harvestable object at the original position and rotation
        GameObject respawnedObject = Instantiate(gameObject, transform.position, transform.rotation);
        NetworkServer.Spawn(respawnedObject);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }
}
