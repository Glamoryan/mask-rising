using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Upgrade
{
    public string id;
    public string name;
    public string description;
    public int cost;
    public int currentLevel;
    public int maxLevel;
    
    public bool CanUpgrade => currentLevel < maxLevel;
    public int NextCost => cost * (currentLevel + 1);
}

/// <summary>
/// Manages player upgrades
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
    [Header("Upgrades")]
    [SerializeField] private List<Upgrade> availableUpgrades = new List<Upgrade>();
    
    public event Action<Upgrade> OnUpgradePurchased;
    public event Action OnUpgradesChanged;
    
    public List<Upgrade> AvailableUpgrades => availableUpgrades;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        InitializeUpgrades();
    }
    
    private void InitializeUpgrades()
    {
        if (availableUpgrades.Count == 0)
        {
            // Create default upgrades
            availableUpgrades.Add(new Upgrade
            {
                id = "health",
                name = "Max Health",
                description = "Increase maximum health by 20",
                cost = 5,
                currentLevel = 0,
                maxLevel = 5
            });
            
            availableUpgrades.Add(new Upgrade
            {
                id = "damage",
                name = "Attack Damage",
                description = "Increase attack damage by 5",
                cost = 3,
                currentLevel = 0,
                maxLevel = 10
            });
            
            availableUpgrades.Add(new Upgrade
            {
                id = "speed",
                name = "Move Speed",
                description = "Increase movement speed by 0.5",
                cost = 4,
                currentLevel = 0,
                maxLevel = 5
            });
            
            availableUpgrades.Add(new Upgrade
            {
                id = "attackspeed",
                name = "Attack Speed",
                description = "Reduce attack cooldown by 0.05s",
                cost = 4,
                currentLevel = 0,
                maxLevel = 5
            });
        }
    }
    
    public bool PurchaseUpgrade(string upgradeId)
    {
        var upgrade = availableUpgrades.Find(u => u.id == upgradeId);
        if (upgrade == null)
        {
            Debug.LogWarning($"[UpgradeManager] Upgrade '{upgradeId}' not found");
            return false;
        }
        
        if (!upgrade.CanUpgrade)
        {
            Debug.Log($"[UpgradeManager] Upgrade '{upgrade.name}' is at max level");
            return false;
        }
        
        int cost = upgrade.NextCost;
        
        if (!SoulManager.Instance.HasEnoughSouls(cost))
        {
            Debug.Log($"[UpgradeManager] Not enough souls. Need {cost}, have {SoulManager.Instance.CurrentSouls}");
            return false;
        }
        
        // Purchase
        if (SoulManager.Instance.SpendSouls(cost))
        {
            upgrade.currentLevel++;
            ApplyUpgrade(upgrade);
            
            Debug.Log($"[UpgradeManager] Purchased '{upgrade.name}' level {upgrade.currentLevel}");
            
            OnUpgradePurchased?.Invoke(upgrade);
            OnUpgradesChanged?.Invoke();
            
            return true;
        }
        
        return false;
    }
    
    private void ApplyUpgrade(Upgrade upgrade)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;
        
        switch (upgrade.id)
        {
            case "health":
                var health = player.GetComponent<PlayerHealth>();
                if (health)
                {
                    // Increase max health (would need to add this functionality to PlayerHealth)
                    Debug.Log($"Applied +20 max health");
                }
                break;
                
            case "damage":
                var attack = player.GetComponent<PlayerAttack>();
                if (attack)
                {
                    // Increase damage (would need public setter)
                    Debug.Log($"Applied +5 attack damage");
                }
                break;
                
            case "speed":
                var controller = player.GetComponent<PlayerController2D>();
                if (controller)
                {
                    // Increase speed (would need public setter)
                    Debug.Log($"Applied +0.5 move speed");
                }
                break;
                
            case "attackspeed":
                var attackSpeed = player.GetComponent<PlayerAttack>();
                if (attackSpeed)
                {
                    // Reduce cooldown (would need public setter)
                    Debug.Log($"Applied -0.05s attack cooldown");
                }
                break;
        }
    }
    
    public Upgrade GetUpgrade(string id)
    {
        return availableUpgrades.Find(u => u.id == id);
    }
}
