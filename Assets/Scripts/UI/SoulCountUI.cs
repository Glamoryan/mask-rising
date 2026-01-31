using TMPro;
using UnityEngine;

/// <summary>
/// Displays soul count in UI
/// </summary>
public class SoulCountUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SoulManager soulManager;
    [SerializeField] private TMP_Text soulText;
    
    [Header("Format")]
    [SerializeField] private string textFormat = "Souls: {0}";
    
    private void Awake()
    {
        if (!soulManager)
            soulManager = FindFirstObjectByType<SoulManager>();
        
        if (!soulText)
            soulText = GetComponent<TMP_Text>();
    }
    
    private void OnEnable()
    {
        if (soulManager)
        {
            soulManager.OnSoulsChanged += UpdateUI;
            UpdateUI(soulManager.CurrentSouls);
        }
    }
    
    private void OnDisable()
    {
        if (soulManager)
            soulManager.OnSoulsChanged -= UpdateUI;
    }
    
    private void UpdateUI(int souls)
    {
        if (soulText)
            soulText.text = string.Format(textFormat, souls);
    }
}
