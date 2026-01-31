using System;
using UnityEngine;

/// <summary>
/// Collectable soul dropped by enemies
/// </summary>
public class Soul : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int soulValue = 1;
    [SerializeField] private float collectRadius = 1.5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float spawnDelay = 1f; // Wait before becoming collectable
    [SerializeField] private float despawnTime = 30f;
    
    private Transform player;
    private bool isMovingToPlayer;
    private bool canBeCollected;
    private float spawnTime;
    
    private void Start()
    {
        spawnTime = Time.time;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        // Check if spawn delay has passed
        if (!canBeCollected)
        {
            if (Time.time - spawnTime >= spawnDelay)
            {
                canBeCollected = true;
            }
            else
            {
                return; // Wait until can be collected
            }
        }
        
        // Despawn after time
        if (Time.time - spawnTime > despawnTime)
        {
            Destroy(gameObject);
            return;
        }
        
        if (!player) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        
        // Start moving to player when in range
        if (distance <= collectRadius)
        {
            isMovingToPlayer = true;
        }
        
        // Move towards player
        if (isMovingToPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            
            // Collect when very close
            if (distance < 0.5f)
            {
                Collect();
            }
        }
    }
    
    private void Collect()
    {
        if (SoulManager.Instance)
        {
            SoulManager.Instance.AddSouls(soulValue);
        }
        
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}
