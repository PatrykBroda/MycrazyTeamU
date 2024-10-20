using UnityEngine;
using Mirror;
using System;

public interface IHealthBonus
{
    int GetHealthBonus(int baseHealth);
    int GetHealthRecoveryBonus();
}

[DisallowMultipleComponent]
public class Health : Energy
{
    // Event to notify subscribers when health reaches zero
    public event Action OnDeath;

    public int baseRecoveryPerTick = 0;
    public int baseHealth = 100;

    // Cache components that give a bonus (attributes, inventory, etc.)
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    IHealthBonus[] _bonusComponents;
    IHealthBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<IHealthBonus>());

    // Current health synchronized across the network
    [SyncVar]
    private int currentHealth;

    public override int max
    {
        get
        {
            // Sum up bonuses manually for performance
            int bonus = 0;
            foreach (IHealthBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetHealthBonus(baseHealth);
            return baseHealth + bonus;
        }
    }

    public override int recoveryPerTick
    {
        get
        {
            // Sum up recovery bonuses manually for performance
            int bonus = 0;
            foreach (IHealthBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetHealthRecoveryBonus();
            return baseRecoveryPerTick + bonus;
        }
    }

    // Property to access current health
    public int CurrentHealth => currentHealth;

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentHealth = max;
    }

    /// <summary>
    /// Applies damage to the entity. If health reaches zero, triggers the OnDeath event.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    [Server]
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0)
            return; // Already dead

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth == 0)
        {
            OnDeath?.Invoke();
        }
    }

    // Optional: Implement healing or recovery methods if needed
}
