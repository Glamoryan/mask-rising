using System;
using UnityEngine;

/// <summary>
/// Manages player soul currency
/// </summary>
public class SoulManager : MonoBehaviour
{
    public static SoulManager Instance { get; private set; }
    
    [Header("Souls")]
    [SerializeField] private int currentSouls = 0;
    
    public event Action<int> OnSoulsChanged;
    public event Action<int> OnSoulsGained;
    public event Action<int> OnSoulsSpent;
    
    public int CurrentSouls => currentSouls;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void AddSouls(int amount)
    {
        if (amount <= 0) return;
        
        currentSouls += amount;
        Debug.Log($"[SoulManager] Gained {amount} souls. Total: {currentSouls}");
        
        OnSoulsGained?.Invoke(amount);
        OnSoulsChanged?.Invoke(currentSouls);
    }
    
    public bool SpendSouls(int amount)
    {
        if (amount <= 0 || currentSouls < amount)
            return false;
        
        currentSouls -= amount;
        Debug.Log($"[SoulManager] Spent {amount} souls. Remaining: {currentSouls}");
        
        OnSoulsSpent?.Invoke(amount);
        OnSoulsChanged?.Invoke(currentSouls);
        
        return true;
    }
    
    public bool HasEnoughSouls(int amount)
    {
        return currentSouls >= amount;
    }
}
