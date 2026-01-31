using UnityEngine;

/// <summary>
/// Spawns artifacts in a defined zone
/// </summary>
public class ArtifactSpawnZone : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject artifactPrefab;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float spawnRadius = 2f;
    
    [Header("Zone Visualization")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private bool showGizmo = true;
    
    private GameObject spawnedArtifact;
    
    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnArtifact();
        }
    }
    
    public void SpawnArtifact()
    {
        if (spawnedArtifact != null)
        {
            Debug.LogWarning($"[{name}] Artifact already spawned in this zone!");
            return;
        }
        
        if (!artifactPrefab)
        {
            Debug.LogError($"[{name}] No artifact prefab assigned!");
            return;
        }
        
        // Random position within radius
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
        
        spawnedArtifact = Instantiate(artifactPrefab, spawnPosition, Quaternion.identity);
        spawnedArtifact.transform.SetParent(transform);
        
        Debug.Log($"[{name}] Spawned artifact at {spawnPosition}");
        
        // Listen to collection event
        var artifact = spawnedArtifact.GetComponent<Artifact>();
        if (artifact)
        {
            artifact.OnCollectionComplete += OnArtifactCollected;
            
            // Notify ArtifactManager
            if (ArtifactManager.Instance)
            {
                ArtifactManager.Instance.RegisterArtifact(artifact);
            }
        }
    }
    
    private void OnArtifactCollected(Artifact artifact)
    {
        Debug.Log($"[{name}] Artifact was collected from this zone");
        spawnedArtifact = null;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw zone label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, name);
        #endif
    }
}
