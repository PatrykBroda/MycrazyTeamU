using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SimpleMonsterSpawner : NetworkBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Name of the monster prefab located in the Resources folder.")]
    public string monsterPrefabName = "DefaultMonster";

    [Tooltip("Array of spawn points where monsters will appear.")]
    public GameObject[] spawnPoints;

    [Tooltip("Time interval (in seconds) between each monster spawn.")]
    public float spawnInterval = 10f;

    [Header("Tracking")]
    [Tooltip("List to keep track of all spawned monsters.")]
    public List<GameObject> spawnedMonsters = new List<GameObject>();

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    void Start()
    {
        // Ensure that spawn points are assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to the SimpleMonsterSpawner.");
            return;
        }

        // Start the spawning process if this is the server
        if (isServer)
        {
            StartSpawning();
        }
    }

    /// <summary>
    /// Initiates the monster spawning coroutine.
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            spawnCoroutine = StartCoroutine(SpawnMonsters());
        }
    }

    /// <summary>
    /// Stops the monster spawning coroutine.
    /// </summary>
    public void StopSpawning()
    {
        if (isSpawning && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            isSpawning = false;
        }
    }

    /// <summary>
    /// Coroutine that handles the spawning of monsters at regular intervals.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnMonsters()
    {
        isSpawning = true;

        while (isSpawning)
        {
            SpawnMonster();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Spawns a single monster at a random spawn point.
    /// </summary>
    private void SpawnMonster()
    {
        if (!isServer)
        {
            Debug.LogWarning("SpawnMonster called on a client. Only the server can spawn monsters.");
            return;
        }

        // Load the monster prefab from the Resources folder
        GameObject monsterPrefab = Resources.Load<GameObject>(monsterPrefabName);

        if (monsterPrefab == null)
        {
            Debug.LogError($"Monster prefab '{monsterPrefabName}' not found in Resources.");
            return;
        }

        // Select a random spawn point
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPosition = spawnPoints[randomIndex].transform.position;
        Quaternion spawnRotation = spawnPoints[randomIndex].transform.rotation;

        // Instantiate the monster on the server
        GameObject spawnedMonster = Instantiate(monsterPrefab, spawnPosition, spawnRotation);

        // Spawn the monster across the network
        NetworkServer.Spawn(spawnedMonster);

        // Add the spawned monster to the tracking list
        spawnedMonsters.Add(spawnedMonster);

        Debug.Log($"Spawned monster '{monsterPrefabName}' at position {spawnPosition}.");
    }

    /// <summary>
    /// Cleans up the spawnedMonsters list by removing references to destroyed monsters.
    /// </summary>
    void Update()
    {
        // Only the server should manage the spawned monsters
        if (!isServer) return;

        // Remove any null references from the list (monsters that have been destroyed)
        spawnedMonsters.RemoveAll(monster => monster == null);
    }

    /// <summary>
    /// Optional: Clean up when the spawner is destroyed.
    /// </summary>
    void OnDestroy()
    {
        StopSpawning();
    }
}
