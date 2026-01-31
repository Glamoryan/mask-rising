using System;
using UnityEngine;

/// <summary>
/// Player health management system
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Damage")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private bool showDamageDebug = true;
    
    [Header("Death")]
    [SerializeField] private bool autoRespawn = false;
    [SerializeField] private float respawnDelay = 3f;
    
    #endregion
    
    #region Events
    
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action<float> OnDamageTaken; // damageAmount
    public event Action<float> OnHealed; // healAmount
    public event Action OnDeath;
    public event Action OnRespawn;
    
    #endregion
    
    #region Properties
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsInvincible { get; private set; }
    
    #endregion
    
    #region Private Fields
    
    private float invincibilityTimer;
    private Vector3 spawnPosition;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
    }
    
    private void Update()
    {
        UpdateInvincibility();
    }
    
    #endregion
    
    #region Health Management
    
    /// <summary>
    /// Take damage from external source
    /// </summary>
    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (IsDead || IsInvincible || amount <= 0)
            return;
        
        currentHealth = Mathf.Max(0, currentHealth - amount);
        
        if (showDamageDebug)
        {
            string attackerName = attacker ? attacker.name : "Unknown";
            Debug.Log($"[PlayerHealth] Took {amount} damage from {attackerName}. Health: {currentHealth}/{maxHealth}");
        }
        
        OnDamageTaken?.Invoke(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Enable invincibility
        IsInvincible = true;
        invincibilityTimer = invincibilityDuration;
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Heal player
    /// </summary>
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0)
            return;
        
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        float actualHealed = currentHealth - oldHealth;
        
        if (actualHealed > 0)
        {
            if (showDamageDebug)
                Debug.Log($"[PlayerHealth] Healed {actualHealed}. Health: {currentHealth}/{maxHealth}");
            
            OnHealed?.Invoke(actualHealed);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    
    /// <summary>
    /// Set health to a specific value
    /// </summary>
    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0 && !IsDead)
            Die();
    }
    
    /// <summary>
    /// Restore to full health
    /// </summary>
    public void FullHeal()
    {
        SetHealth(maxHealth);
    }
    
    #endregion
    
    #region Death & Respawn
    
    private void Die()
    {
        if (showDamageDebug)
            Debug.Log("[PlayerHealth] Player died!");
        
        OnDeath?.Invoke();
        
        // TODO: Add death animation
        // TODO: Add death sound
        // TODO: Disable player controls
        
        if (autoRespawn)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
    }
    
    public void Respawn()
    {
        currentHealth = maxHealth;
        transform.position = spawnPosition;
        IsInvincible = false;
        invincibilityTimer = 0f;
        
        if (showDamageDebug)
            Debug.Log("[PlayerHealth] Player respawned!");
        
        OnRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // TODO: Add respawn effect
        // TODO: Re-enable player controls
    }
    
    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
    }
    
    #endregion
    
    #region Invincibility
    
    private void UpdateInvincibility()
    {
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            if (invincibilityTimer <= 0)
            {
                IsInvincible = false;
            }
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Check if player can take damage
    /// </summary>
    public bool CanTakeDamage()
    {
        return !IsDead && !IsInvincible;
    }
    
    /// <summary>
    /// Instantly kill the player
    /// </summary>
    public void InstantKill()
    {
        TakeDamage(currentHealth);
    }
    
    #endregion
    
    #region Debug
    
    [ContextMenu("Debug: Take 10 Damage")]
    private void DebugTakeDamage()
    {
        TakeDamage(10f);
    }
    
    [ContextMenu("Debug: Heal 25 HP")]
    private void DebugHeal()
    {
        Heal(25f);
    }
    
    [ContextMenu("Debug: Kill Player")]
    private void DebugKill()
    {
        InstantKill();
    }
    
    [ContextMenu("Debug: Respawn")]
    private void DebugRespawn()
    {
        Respawn();
    }
    
    #endregion
}
