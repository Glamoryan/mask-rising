using System;
using UnityEngine;

/// <summary>
/// Tracks collected artifacts count
/// </summary>
public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }
    
    [Header("Stats")]
    [SerializeField] private int collectedCount = 0;
    [SerializeField] private int totalArtifacts = 0;
    
    public event Action<int, int> OnArtifactCollected; // collected, total
    
    public int CollectedCount => collectedCount;
    public int TotalArtifacts => totalArtifacts;
    public bool AllCollected => collectedCount >= totalArtifacts && totalArtifacts > 0;
    
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        // Count all artifacts in scene
        CountTotalArtifacts();
        
        // Subscribe to existing artifacts
        SubscribeToArtifacts();
        
        // Wait a frame for spawn zones to spawn, then subscribe again
        Invoke(nameof(SubscribeToArtifacts), 0.1f);
    }
    
    private void CountTotalArtifacts()
    {
        var zones = FindObjectsByType<ArtifactSpawnZone>(FindObjectsSortMode.None);
        totalArtifacts = zones.Length;
        
        Debug.Log($"[ArtifactManager] Found {totalArtifacts} artifact zones");
    }
    
    private void SubscribeToArtifacts()
    {
        var artifacts = FindObjectsByType<Artifact>(FindObjectsSortMode.None);
        Debug.Log($"[ArtifactManager] Found {artifacts.Length} artifacts to subscribe");
        
        foreach (var artifact in artifacts)
        {
            // Avoid double subscription
            artifact.OnCollectionComplete -= HandleArtifactCollected;
            artifact.OnCollectionComplete += HandleArtifactCollected;
        }
    }
    
    public void RegisterArtifact(Artifact artifact)
    {
        if (artifact == null) return;
        
        artifact.OnCollectionComplete -= HandleArtifactCollected;
        artifact.OnCollectionComplete += HandleArtifactCollected;
        Debug.Log($"[ArtifactManager] Registered artifact {artifact.name}");
    }
    
    private void HandleArtifactCollected(Artifact artifact)
    {
        collectedCount++;
        
        Debug.Log($"[ArtifactManager] Collected {collectedCount}/{totalArtifacts} artifacts");
        
        OnArtifactCollected?.Invoke(collectedCount, totalArtifacts);
        
        if (AllCollected)
        {
            Debug.Log("[ArtifactManager] All artifacts collected!");
            // TODO: Trigger win condition or next objective
        }
    }
    
    public void ResetCount()
    {
        collectedCount = 0;
        OnArtifactCollected?.Invoke(collectedCount, totalArtifacts);
    }
}
