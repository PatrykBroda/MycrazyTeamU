using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class CentralizedMonsterSpawner : NetworkBehaviour
{
    public string monsterPrefabName = "DefaultMonster";  // Name of the monster prefab in Resources
    public GameObject[] spawnPoints;  // Array of spawn points
    public float spawnInterval = 10f;  // Interval in seconds for spawning monsters
    public List<NavMeshAgent> spawnedMonsters = new List<NavMeshAgent>();  // List to track spawned monsters
    public float attackRange = 2f;     // Attack range for monsters
    public float attackCooldown = 1f;  // Time between attacks
    public float minDamage = 2f;       // Minimum damage to inflict
    public float maxDamage = 5f;       // Maximum damage to inflict
    public List<GameObject> monsterTargets = new List<GameObject>(); // Public list to track monster targets
    public float maxFollowRange = 20f;  // Maximum distance at which monsters will follow a player

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned.");
            return;
        }

        StartCoroutine(CheckForSpawning());
    }

    private IEnumerator CheckForSpawning()
    {
        while (true)
        {
            if (!isSpawning)
            {
                spawnCoroutine = StartCoroutine(SpawnMonsters());
            }
            yield return new WaitForSeconds(1f);  // Check every second
        }
    }

    private IEnumerator SpawnMonsters()
    {
        isSpawning = true;
        while (isSpawning)
        {
            SpawnMonster();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnMonster()
    {
        if (!NetworkServer.active) return;  // Ensure this only runs on the server

        // Load the monster prefab from the Resources folder
        GameObject monsterPrefab = Resources.Load<GameObject>(monsterPrefabName);

        if (monsterPrefab == null)
        {
            Debug.LogError("Monster prefab not found in Resources: " + monsterPrefabName);
            return;
        }

        // Choose a random spawn point
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPosition = spawnPoints[randomIndex].transform.position;

        // Instantiate the monster at the selected spawn point
        GameObject spawnedMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);

        // For multiplayer games, ensure the monster is spawned on the network
        NetworkServer.Spawn(spawnedMonster);

        // Add NavMeshAgent if not already attached
        var navMeshAgent = spawnedMonster.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = spawnedMonster.AddComponent<NavMeshAgent>();
        }

        // Disable auto-braking to allow continuous movement
        navMeshAgent.autoBraking = false;

        // Ensure NetworkAnimator is attached and properly initialized
        NetworkAnimator networkAnimator = spawnedMonster.GetComponent<NetworkAnimator>();
        if (networkAnimator == null)
        {
            networkAnimator = spawnedMonster.AddComponent<NetworkAnimator>();
            Debug.Log("NetworkAnimator was missing and has been added.");
        }

        // Add the monster to the list
        spawnedMonsters.Add(navMeshAgent);

        // Set an initial wandering destination for the monster
        Wander(navMeshAgent);
    }

    void Update()
    {
        // Clean up any destroyed monsters from the list
        spawnedMonsters.RemoveAll(monster => monster == null);

        foreach (var agent in spawnedMonsters)
        {
            if (agent == null)
                continue;

            GameObject target = FindClosestPlayer(agent.transform.position);

            if (target != null)
            {
                float distanceToTarget = Vector3.Distance(agent.transform.position, target.transform.position);

                if (distanceToTarget <= attackRange)
                {
                    if (!monsterTargets.Contains(target))
                    {
                        monsterTargets.Add(target);  // Add the target to the list if it's not already there
                    }

                    AttackTarget(agent, target);  // Trigger the attack logic
                }

                if (distanceToTarget <= maxFollowRange)
                {
                    // Follow the player
                    agent.SetDestination(target.transform.position);
                }
                else
                {
                    // Player is beyond maxFollowRange, start wandering
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        Wander(agent);
                    }
                }
            }
            else
            {
                // No player found, start wandering
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Wander(agent);
                }
            }

            // Update animator
            UpdateAnimator(agent);
        }
    }

    private void Wander(NavMeshAgent agent)
    {
        float wanderRadius = 10f; // Adjust the radius as needed
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += agent.transform.position;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
        else
        {
            Debug.LogError("Failed to find a valid NavMesh position for monster wandering.");
        }
    }

    private GameObject FindClosestPlayer(Vector3 agentPosition)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(agentPosition, player.transform.position);
            if (distanceToPlayer < shortestDistance)
            {
                shortestDistance = distanceToPlayer;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    private void AttackTarget(NavMeshAgent agent, GameObject target)
    {
        if (!isServer) return;  // Ensure this runs only on the server

        // Cooldown management for the attack
        float lastAttackTime = agent.gameObject.GetComponent<MonsterAttackCooldown>()?.LastAttackTime ?? -attackCooldown;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // Play attack animation
            NetworkAnimator animator = agent.GetComponent<NetworkAnimator>();
            if (animator != null)
            {
                Debug.Log("Triggering attack animation for monster.");
                animator.SetTrigger("Attack");  // Trigger the attack animation
            }
            else
            {
                Debug.LogError("Missing NetworkAnimator on the monster.");
            }

            // Deal damage to the player
            DamageTarget(target);

            // Update the last attack time
            if (agent.gameObject.GetComponent<MonsterAttackCooldown>() == null)
            {
                agent.gameObject.AddComponent<MonsterAttackCooldown>().LastAttackTime = Time.time;
            }
            else
            {
                agent.gameObject.GetComponent<MonsterAttackCooldown>().LastAttackTime = Time.time;
            }
        }
    }

    private void DamageTarget(GameObject target)
    {
        if (!isServer) return;  // Ensure this runs only on the server

        int damage = Mathf.RoundToInt(Random.Range(minDamage, maxDamage));

        // Find the Health component in the target or its children
        var healthComponent = target.GetComponentInChildren<Health>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damage);  // Apply damage
        }
        else
        {
            Debug.LogError("Target or its children do not have a Health component.");
        }
    }

    private void UpdateAnimator(NavMeshAgent agent)
    {
        // Find the NetworkAnimator for this agent (monster)
        NetworkAnimator animator = agent.GetComponent<NetworkAnimator>();
        if (animator != null)
        {
            // Update the movement animation based on the agent's velocity
            float maxSpeed = agent.speed;
            float currentSpeed = agent.velocity.magnitude;
            float normalizedSpeed = currentSpeed / maxSpeed;

            animator.animator.SetFloat("Movement", normalizedSpeed);  // Update movement parameter in animator
        }
        else
        {
            Debug.LogError("Missing NetworkAnimator for monster. Animations may not work correctly.");
        }
    }
}

// Helper class to store the last attack time per monster
public class MonsterAttackCooldown : MonoBehaviour
{
    public float LastAttackTime { get; set; }
}
