using System;
using UnityEngine;

/// <summary>
/// Collectable artifact that requires holding E key
/// </summary>
public class Artifact : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private float collectionTime = 3f;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode collectKey = KeyCode.E;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private bool showGizmos = true;
    
    // Events (Header not allowed on events)
    public event Action<Artifact> OnCollectionStarted;
    public event Action<Artifact, float> OnCollectionProgress; // artifact, progress (0-1)
    public event Action<Artifact> OnCollectionComplete;
    public event Action<Artifact> OnCollectionCancelled;
    
    #region Properties
    
    public bool IsBeingCollected { get; private set; }
    public float CollectionProgress { get; private set; }
    public bool IsCollected { get; private set; }
    
    #endregion
    
    #region Private Fields
    
    private Transform playerInRange;
    private float collectionTimer;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        if (!visual)
            visual = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void Update()
    {
        if (IsCollected) return;
        
        CheckPlayerInRange();
        HandleCollection();
    }
    
    #endregion
    
    #region Collection Logic
    
    private void CheckPlayerInRange()
    {
        // Find player
        if (!playerInRange)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance <= interactionRange)
                {
                    playerInRange = player.transform;
                }
            }
        }
        else
        {
            // Check if still in range
            float distance = Vector2.Distance(transform.position, playerInRange.position);
            if (distance > interactionRange)
            {
                if (IsBeingCollected)
                    CancelCollection();
                playerInRange = null;
            }
        }
    }
    
    private void HandleCollection()
    {
        if (!playerInRange) return;
        
        // Check if E key is held
        if (Input.GetKey(collectKey))
        {
            if (!IsBeingCollected)
            {
                StartCollection();
            }
            
            UpdateCollection();
        }
        else
        {
            if (IsBeingCollected)
            {
                CancelCollection();
            }
        }
    }
    
    private void StartCollection()
    {
        IsBeingCollected = true;
        collectionTimer = 0f;
        OnCollectionStarted?.Invoke(this);
        
        Debug.Log($"[Artifact] Started collecting {name}");
    }
    
    private void UpdateCollection()
    {
        collectionTimer += Time.deltaTime;
        CollectionProgress = Mathf.Clamp01(collectionTimer / collectionTime);
        
        OnCollectionProgress?.Invoke(this, CollectionProgress);
        
        // Check if complete
        if (collectionTimer >= collectionTime)
        {
            CompleteCollection();
        }
    }
    
    private void CompleteCollection()
    {
        IsBeingCollected = false;
        IsCollected = true;
        
        Debug.Log($"[Artifact] Collected {name}!");
        
        OnCollectionComplete?.Invoke(this);
        
        // Visual feedback
        if (visual)
            visual.enabled = false;
        
        // Disable collider
        var collider = GetComponent<Collider2D>();
        if (collider)
            collider.enabled = false;
        
        // Destroy after a moment
        Destroy(gameObject, 0.5f);
    }
    
    private void CancelCollection()
    {
        IsBeingCollected = false;
        collectionTimer = 0f;
        CollectionProgress = 0f;
        
        OnCollectionCancelled?.Invoke(this);
        
        Debug.Log($"[Artifact] Cancelled collecting {name}");
    }
    
    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw interaction range
        Gizmos.color = IsCollected ? Color.gray : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    
    #endregion
}
