using UnityEngine;

/// <summary>
/// Player attack system with melee detection
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Input")]
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
    
    [Header("Visual")]
    [SerializeField] private bool showAttackGizmos = true;
    
    [Header("Debug")]
    [SerializeField] private bool logAttacks = true;
    
    #endregion
    
    #region Private Fields
    
    private Rigidbody2D rb;
    private float lastAttackTime;
    private Vector2 lastMoveDirection = Vector2.down;
    
    #endregion
    
    #region Properties
    
    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Set default enemy layer if not set
        if (enemyLayer == 0)
            enemyLayer = LayerMask.GetMask("Default");
    }
    
    private void Update()
    {
        HandleInput();
        TrackMoveDirection();
    }
    
    #endregion
    
    #region Input
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(attackKey))
        {
            TryAttack();
        }
    }
    
    #endregion
    
    #region Attack
    
    private void TryAttack()
    {
        if (!CanAttack)
        {
            if (logAttacks)
                Debug.Log($"[PlayerAttack] Attack on cooldown. Wait {(lastAttackTime + attackCooldown - Time.time):F1}s");
            return;
        }
        
        PerformAttack();
    }
    
    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        // Calculate attack position (in front of player)
        Vector2 attackPosition = rb.position + (lastMoveDirection.normalized * attackRange * 0.5f);
        
        // Detect enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);
        
        if (logAttacks)
            Debug.Log($"[PlayerAttack] Attack! Found {hits.Length} targets");
        
        int hitCount = 0;
        foreach (var hit in hits)
        {
            // Skip self
            if (hit.gameObject == gameObject)
                continue;
            
            // Try to damage enemy
            var enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth)
            {
                enemyHealth.TakeDamage(attackDamage, gameObject);
                hitCount++;
                
                if (logAttacks)
                    Debug.Log($"[PlayerAttack] Hit {hit.name} for {attackDamage} damage");
            }
        }
        
        if (logAttacks && hitCount == 0)
            Debug.Log("[PlayerAttack] Attack missed - no enemies in range");
        
        // TODO: Play attack animation
        // TODO: Play attack sound
        // TODO: Spawn attack effect
    }
    
    #endregion
    
    #region Movement Tracking
    
    private void TrackMoveDirection()
    {
        // Track movement direction for attack direction
        Vector2 velocity = rb.linearVelocity;
        
        if (velocity.magnitude > 0.1f)
        {
            lastMoveDirection = velocity.normalized;
        }
    }
    
    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (!showAttackGizmos) return;
        
        Vector2 playerPos = transform.position;
        
        // Draw attack range
        Gizmos.color = CanAttack ? Color.green : Color.red;
        Gizmos.DrawWireSphere(playerPos, attackRange);
        
        // Draw attack direction
        if (Application.isPlaying)
        {
            Vector2 attackPos = playerPos + (lastMoveDirection.normalized * attackRange * 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPos, attackRange);
            Gizmos.DrawLine(playerPos, attackPos);
        }
    }
    
    #endregion
}
