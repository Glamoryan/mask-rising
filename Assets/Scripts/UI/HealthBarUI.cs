using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual health bar UI that updates automatically
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Slider healthSlider; // Option 1: Using Slider
    [SerializeField] private Image fillImage;      // Option 2: Using Image
    [SerializeField] private TMP_Text healthText;
    
    [Header("Visual Settings")]
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private bool showDecimals = false;
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationSpeed = 5f;
    
    private float targetFillAmount;
    private float currentFillAmount;
    
    private void Awake()
    {
        // Auto-find player health if not assigned
        if (!playerHealth)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        // Auto-find Slider if available
        if (!healthSlider && !fillImage)
            healthSlider = GetComponent<Slider>();
        
        // Auto-find UI components if not assigned
        if (!fillImage && !healthSlider)
            fillImage = GetComponentInChildren<Image>();
        
        if (!healthText)
            healthText = GetComponentInChildren<TMP_Text>();
        
        // Setup slider if using it
        if (healthSlider && playerHealth)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.wholeNumbers = true;
        }
        
        // Setup default gradient if none assigned
        if (healthGradient == null || healthGradient.colorKeys.Length == 0)
            SetupDefaultGradient();
    }
    
    private void OnEnable()
    {
        if (playerHealth)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            // Initialize immediately
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }
    
    private void OnDisable()
    {
        if (playerHealth)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }
    
    private void Update()
    {
        if (animateChanges)
            AnimateFillAmount();
    }
    
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthPercentage = currentHealth / maxHealth;
        targetFillAmount = healthPercentage;
        
        // Update Slider if using it
        if (healthSlider)
        {
            if (animateChanges)
            {
                // Animated slider update handled in Update()
            }
            else
            {
                healthSlider.value = currentHealth;
            }
            
            // Update slider fill color
            var fillImage = healthSlider.fillRect?.GetComponent<Image>();
            if (fillImage)
                fillImage.color = healthGradient.Evaluate(healthPercentage);
        }
        // Update Image if using it
        else if (fillImage)
        {
            if (!animateChanges)
            {
                currentFillAmount = targetFillAmount;
                fillImage.fillAmount = currentFillAmount;
            }
            
            UpdateHealthColor(healthPercentage);
        }
        
        UpdateHealthText(currentHealth, maxHealth);
    }
    
    private void AnimateFillAmount()
    {
        if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * animationSpeed);
            
            // Update slider
            if (healthSlider && playerHealth)
            {
                healthSlider.value = currentFillAmount * playerHealth.MaxHealth;
            }
            // Update image
            else if (fillImage)
            {
                fillImage.fillAmount = currentFillAmount;
            }
        }
    }
    
    private void UpdateHealthColor(float healthPercentage)
    {
        if (fillImage && healthGradient != null)
        {
            fillImage.color = healthGradient.Evaluate(healthPercentage);
        }
    }
    
    private void UpdateHealthText(float currentHealth, float maxHealth)
    {
        if (!healthText) return;
        
        if (showDecimals)
        {
            healthText.text = $"{currentHealth:F1} / {maxHealth:F1}";
        }
        else
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {Mathf.Ceil(maxHealth)}";
        }
    }
    
    private void SetupDefaultGradient()
    {
        healthGradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0].color = Color.red;      // 0% health
        colorKeys[0].time = 0.0f;
        colorKeys[1].color = Color.yellow;   // 50% health
        colorKeys[1].time = 0.5f;
        colorKeys[2].color = Color.green;    // 100% health
        colorKeys[2].time = 1.0f;
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;
        
        healthGradient.SetKeys(colorKeys, alphaKeys);
    }
}
