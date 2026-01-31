using System;
using UnityEngine;

/// <summary>
/// Enemy health management system
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth;
    
    [Header("Damage")]
    [SerializeField] private float invincibilityDuration = 0.2f;
    [SerializeField] private bool showDamageDebug = false;
    
    [Header("Death")]
    [SerializeField] private float despawnDelay = 2f;
    [SerializeField] private bool dropLoot = false;
    
    #endregion
    
    #region Events
    
    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnDamageTaken;
    public event Action OnDeath;
    
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
    private SpriteRenderer visual;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        currentHealth = maxHealth;
        visual = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void Update()
    {
        UpdateInvincibility();
    }
    
    #endregion
    
    #region Health Management
    
    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (IsDead || IsInvincible || amount <= 0)
            return;
        
        currentHealth = Mathf.Max(0, currentHealth - amount);
        
        if (showDamageDebug)
        {
            string attackerName = attacker ? attacker.name : "Unknown";
            Debug.Log($"[{name}] Took {amount} damage from {attackerName}. Health: {currentHealth}/{maxHealth}");
        }
        
        OnDamageTaken?.Invoke(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Visual feedback
        FlashDamage();
        
        // Enable invincibility
        IsInvincible = true;
        invincibilityTimer = invincibilityDuration;
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0)
            return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    #endregion
    
    #region Death
    
    private void Die()
    {
        if (showDamageDebug)
            Debug.Log($"[{name}] Enemy died!");
        
        OnDeath?.Invoke();
        
        // Disable AI
        var ai = GetComponent<EnemyHumanAI>();
        if (ai) ai.enabled = false;
        
        // Disable collisions
        var collider = GetComponent<Collider2D>();
        if (collider) collider.enabled = false;
        
        // TODO: Add death animation
        // TODO: Add death sound
        // TODO: Spawn loot
        
        // Destroy after delay
        Destroy(gameObject, despawnDelay);
    }
    
    #endregion
    
    #region Visual Feedback
    
    private void FlashDamage()
    {
        if (visual)
        {
            // Simple flash effect (can be improved with coroutine)
            StartCoroutine(FlashCoroutine());
        }
    }
    
    private System.Collections.IEnumerator FlashCoroutine()
    {
        Color original = visual.color;
        visual.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        visual.color = original;
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
}
