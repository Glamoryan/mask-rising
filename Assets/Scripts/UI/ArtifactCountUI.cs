using TMPro;
using UnityEngine;

/// <summary>
/// Displays artifact collection count in UI
/// </summary>
public class ArtifactCountUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArtifactManager artifactManager;
    [SerializeField] private TMP_Text countText;
    
    [Header("Format")]
    [SerializeField] private string textFormat = "Artifacts: {0}/{1}";
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color completedColor = Color.green;
    
    private void Awake()
    {
        if (!artifactManager)
            artifactManager = FindFirstObjectByType<ArtifactManager>();
        
        if (!countText)
            countText = GetComponent<TMP_Text>();
    }
    
    private void OnEnable()
    {
        if (artifactManager)
        {
            artifactManager.OnArtifactCollected += UpdateUI;
            // Initialize
            UpdateUI(artifactManager.CollectedCount, artifactManager.TotalArtifacts);
        }
    }
    
    private void OnDisable()
    {
        if (artifactManager)
            artifactManager.OnArtifactCollected -= UpdateUI;
    }
    
    private void UpdateUI(int collected, int total)
    {
        if (!countText) return;
        
        countText.text = string.Format(textFormat, collected, total);
        
        // Change color if all collected
        if (collected >= total && total > 0)
        {
            countText.color = completedColor;
        }
        else
        {
            countText.color = defaultColor;
        }
    }
}
