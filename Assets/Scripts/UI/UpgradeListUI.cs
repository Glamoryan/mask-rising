using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays current upgrades in UI
/// </summary>
public class UpgradeListUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private TMP_Text upgradeText;
    
    [Header("Format")]
    [SerializeField] private bool showCosts = true;
    [SerializeField] private bool showLevels = true;
    
    private void Awake()
    {
        if (!upgradeManager)
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
        
        if (!upgradeText)
            upgradeText = GetComponent<TMP_Text>();
    }
    
    private void OnEnable()
    {
        if (upgradeManager)
        {
            upgradeManager.OnUpgradesChanged += UpdateUI;
            upgradeManager.OnUpgradePurchased += OnUpgradePurchased;
            UpdateUI();
        }
    }
    
    private void OnDisable()
    {
        if (upgradeManager)
        {
            upgradeManager.OnUpgradesChanged -= UpdateUI;
            upgradeManager.OnUpgradePurchased -= OnUpgradePurchased;
        }
    }
    
    private void UpdateUI()
    {
        if (!upgradeText || !upgradeManager) return;
        
        var sb = new StringBuilder();
        sb.AppendLine("<b>Upgrades:</b>");
        
        foreach (var upgrade in upgradeManager.AvailableUpgrades)
        {
            if (upgrade.currentLevel > 0)
            {
                sb.Append($"• {upgrade.name}");
                
                if (showLevels)
                {
                    sb.Append($" Lv.{upgrade.currentLevel}");
                    
                    if (upgrade.CanUpgrade && showCosts)
                    {
                        sb.Append($" (Next: {upgrade.NextCost} souls)");
                    }
                    else if (!upgrade.CanUpgrade)
                    {
                        sb.Append($" <color=yellow>(MAX)</color>");
                    }
                }
                
                sb.AppendLine();
            }
        }
        
        if (sb.Length == "<b>Upgrades:</b>\n".Length)
        {
            sb.AppendLine("<i>No upgrades yet</i>");
        }
        
        upgradeText.text = sb.ToString();
    }
    
    private void OnUpgradePurchased(Upgrade upgrade)
    {
        UpdateUI();
    }
}
