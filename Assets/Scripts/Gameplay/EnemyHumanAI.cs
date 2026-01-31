using UnityEngine;

/// <summary>
/// Enemy AI with state-based behavior (Idle, Chase, Attack)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHumanAI : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer visual;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float loseTargetDistance = 8f;
    
    [Header("Patrol")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Waypoints;
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float waypointReachDistance = 0.5f;
    [SerializeField] private float waypointWaitTime = 2f;
    [SerializeField] private bool loopWaypoints = true;
    
    [Header("Wander (Random Patrol)")]
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderChangeInterval = 3f;
    
    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logStateChanges = false;
    
    #endregion
    
    #region Private Fields
    
    private Rigidbody2D rb;
    private AIState currentState;
    private float lastAttackTime;
    private float distanceToTarget;
    private Vector2 directionToTarget;
    
    // Patrol state
    private int currentWaypointIndex;
    private float waypointWaitTimer;
    private Vector2 wanderTarget;
    private float wanderTimer;
    private Vector2 patrolOrigin;
    
    #endregion
    
    #region States
    
    public enum AIState
    {
        Patrol,
        Chase,
        Attack
    }
    
    public enum PatrolMode
    {
        Waypoints,
        RandomWander,
        Stationary
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeComponents();
        SetupPhysics();
    }
    
    private void Start()
    {
        patrolOrigin = rb.position;
        InitializePatrol();
        ChangeState(AIState.Patrol);
    }
    
    private void Update()
    {
        UpdateTargetInfo();
        UpdateStateMachine();
    }
    
    private void FixedUpdate()
    {
        ExecuteCurrentState();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (!visual)
            visual = GetComponentInChildren<SpriteRenderer>();
        
        if (!target)
            FindPlayer();
    }
    
    private void SetupPhysics()
    {
        // Top-down 2D setup
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    private void FindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
            target = player.transform;
        else
            Debug.LogWarning($"[{name}] Player not found!", this);
    }
    
    private void InitializePatrol()
    {
        if (patrolMode == PatrolMode.Waypoints && patrolWaypoints.Length > 0)
        {
            currentWaypointIndex = 0;
        }
        else if (patrolMode == PatrolMode.RandomWander)
        {
            PickNewWanderTarget();
        }
    }
    
    #endregion
    
    #region State Machine
    
    private void UpdateTargetInfo()
    {
        if (!target) return;
        
        Vector2 currentPos = rb.position;
        Vector2 targetPos = target.position;
        
        directionToTarget = (targetPos - currentPos).normalized;
        distanceToTarget = Vector2.Distance(currentPos, targetPos);
    }
    
    private void UpdateStateMachine()
    {
        if (!target) return;
        
        switch (currentState)
        {
            case AIState.Patrol:
                if (distanceToTarget <= detectionRange)
                    ChangeState(AIState.Chase);
                break;
                
            case AIState.Chase:
                if (distanceToTarget > loseTargetDistance)
                    ChangeState(AIState.Patrol);
                else if (distanceToTarget <= attackRange)
                    ChangeState(AIState.Attack);
                break;
                
            case AIState.Attack:
                if (distanceToTarget > attackRange)
                    ChangeState(AIState.Chase);
                break;
        }
    }
    
    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                ExecutePatrol();
                break;
                
            case AIState.Chase:
                ExecuteChase();
                break;
                
            case AIState.Attack:
                ExecuteAttack();
                break;
        }
    }
    
    private void ChangeState(AIState newState)
    {
        if (currentState == newState) return;
        
        // Exit current state
        OnStateExit(currentState);
        
        // Log state change
        if (logStateChanges)
            Debug.Log($"[{name}] State: {currentState} → {newState}");
        
        currentState = newState;
        
        // Enter new state
        OnStateEnter(newState);
    }
    
    private void OnStateEnter(AIState state)
    {
        switch (state)
        {
            case AIState.Patrol:
                // Reset patrol when returning to it
                waypointWaitTimer = 0f;
                break;
                
            case AIState.Chase:
                // Could trigger chase animation/sound here
                break;
                
            case AIState.Attack:
                // Reset attack timer
                lastAttackTime = Time.time - attackCooldown;
                break;
        }
    }
    
    private void OnStateExit(AIState state)
    {
        // Cleanup when leaving state
    }
    
    #endregion
    
    #region State Behaviors
    
    private void ExecutePatrol()
    {
        switch (patrolMode)
        {
            case PatrolMode.Waypoints:
                PatrolWaypoints();
                break;
                
            case PatrolMode.RandomWander:
                WanderRandomly();
                break;
                
            case PatrolMode.Stationary:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }
    
    private void PatrolWaypoints()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Waiting at waypoint
        if (waypointWaitTimer > 0)
        {
            waypointWaitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Get current waypoint
        Transform targetWaypoint = patrolWaypoints[currentWaypointIndex];
        if (!targetWaypoint)
        {
            MoveToNextWaypoint();
            return;
        }
        
        Vector2 waypointPos = targetWaypoint.position;
        Vector2 direction = (waypointPos - rb.position).normalized;
        float distance = Vector2.Distance(rb.position, waypointPos);
        
        // Reached waypoint
        if (distance <= waypointReachDistance)
        {
            waypointWaitTimer = waypointWaitTime;
            MoveToNextWaypoint();
            return;
        }
        
        // Move towards waypoint
        rb.linearVelocity = direction * patrolSpeed;
        
        // Update visual
        if (visual && Mathf.Abs(direction.x) > 0.1f)
            visual.flipX = direction.x < 0;
    }
    
    private void MoveToNextWaypoint()
    {
        currentWaypointIndex++;
        
        if (currentWaypointIndex >= patrolWaypoints.Length)
        {
            if (loopWaypoints)
                currentWaypointIndex = 0;
            else
                currentWaypointIndex = patrolWaypoints.Length - 1;
        }
    }
    
    private void WanderRandomly()
    {
        wanderTimer -= Time.fixedDeltaTime;
        
        // Pick new target
        if (wanderTimer <= 0)
        {
            PickNewWanderTarget();
            wanderTimer = wanderChangeInterval;
        }
        
        // Move to wander target
        Vector2 direction = (wanderTarget - rb.position).normalized;
        float distance = Vector2.Distance(rb.position, wanderTarget);
        
        if (distance < 0.5f)
        {
            PickNewWanderTarget();
        }
        else
        {
            rb.linearVelocity = direction * patrolSpeed;
            
            // Update visual
            if (visual && Mathf.Abs(direction.x) > 0.1f)
                visual.flipX = direction.x < 0;
        }
    }
    
    private void PickNewWanderTarget()
    {
        // Pick random point within wander radius
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = patrolOrigin + randomOffset;
    }
    
    private void ExecuteChase()
    {
        // Move towards target
        Vector2 velocity = directionToTarget * moveSpeed;
        rb.linearVelocity = velocity;
        
        // Flip sprite based on direction
        UpdateVisualDirection();
    }
    
    private void ExecuteAttack()
    {
        // Stop movement during attack
        rb.linearVelocity = Vector2.zero;
        
        // Try to attack
        if (CanAttack())
        {
            PerformAttack();
        }
        
        // Still face the target
        UpdateVisualDirection();
    }
    
    #endregion
    
    #region Combat
    
    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }
    
    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (!target) return;
        
        // Try to get PlayerHealth component
        var playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth)
        {
            playerHealth.TakeDamage(attackDamage, gameObject);
            
            if (logStateChanges)
                Debug.Log($"[{name}] Dealt {attackDamage} damage to player");
        }
        else
        {
            Debug.LogWarning($"[{name}] Target has no PlayerHealth component!", this);
        }
        
        // TODO: Add animation trigger
        // TODO: Add attack sound
    }
    
    #endregion
    
    #region Visual
    
    private void UpdateVisualDirection()
    {
        if (!visual) return;
        
        // Flip sprite based on horizontal direction
        if (Mathf.Abs(directionToTarget.x) > 0.1f)
        {
            visual.flipX = directionToTarget.x < 0;
        }
    }
    
    #endregion
    
    #region Debug & Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Vector3 pos = transform.position;
        
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, attackRange);
        
        // Lose target range
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(pos, loseTargetDistance);
        
        // Direction to target
        if (target && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, target.position);
        }
        
        // Patrol visualization
        DrawPatrolGizmos();
    }
    
    private void DrawPatrolGizmos()
    {
        if (patrolMode == PatrolMode.Waypoints && patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            // Draw waypoint path
            Gizmos.color = Color.green;
            
            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                if (!patrolWaypoints[i]) continue;
                
                Vector3 wpPos = patrolWaypoints[i].position;
                
                // Draw waypoint sphere
                Gizmos.DrawWireSphere(wpPos, 0.3f);
                
                // Draw line to next waypoint
                int nextIndex = (i + 1) % patrolWaypoints.Length;
                if (nextIndex < patrolWaypoints.Length && patrolWaypoints[nextIndex])
                {
                    Vector3 nextPos = patrolWaypoints[nextIndex].position;
                    Gizmos.DrawLine(wpPos, nextPos);
                }
                
                // Draw current waypoint indicator
                if (Application.isPlaying && i == currentWaypointIndex)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(wpPos, 0.2f);
                    Gizmos.color = Color.green;
                }
            }
        }
        else if (patrolMode == PatrolMode.RandomWander)
        {
            // Draw wander radius
            Vector3 origin = Application.isPlaying ? (Vector3)patrolOrigin : transform.position;
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(origin, wanderRadius);
            
            // Draw current wander target
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(wanderTarget, 0.2f);
                Gizmos.DrawLine(transform.position, wanderTarget);
            }
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Get current AI state
    /// </summary>
    public AIState CurrentState => currentState;
    
    /// <summary>
    /// Force set target (useful for scripted events)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Check if enemy is currently aggressive
    /// </summary>
    public bool IsAggressive => currentState != AIState.Patrol;
    
    #endregion
}
