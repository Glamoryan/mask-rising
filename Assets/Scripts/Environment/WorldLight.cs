using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class WorldLight : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    
    private Light2D _light;
    private Gradient gradient;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
        if (!cycle) cycle = FindFirstObjectByType<DayNightCycle>();
        
        // Create gradient with custom colors
        gradient = CreateDefaultGradient();
    }

    private void OnEnable()
    {
        if (cycle) cycle.OnTick += OnPhaseUpdate;
    }

    private void OnDisable()
    {
        if (cycle) cycle.OnTick -= OnPhaseUpdate;
    }

    private void OnPhaseUpdate(DayPhase phase, float t)
    {
        if (!_light || gradient == null) return;

        // Map time to gradient based on 24-hour clock
        // 06:00 (day start) = 0.0
        // 12:00 (noon) = 0.25
        // 18:00 (night start) = 0.5
        // 00:00 (midnight) = 0.75
        // 06:00 (next day) = 1.0
        
        float hour = cycle.CurrentHour;
        float gradientTime;
        
        // Normalize hour (6-30) to gradient time (0-1)
        if (hour >= 6f)
        {
            // 06:00 to 23:59 -> map to 0.0 to 0.75
            gradientTime = (hour - 6f) / 24f;
        }
        else
        {
            // 00:00 to 05:59 -> map to 0.75 to 1.0
            gradientTime = 0.75f + (hour / 24f);
        }

        _light.color = gradient.Evaluate(gradientTime);
    }

    private Gradient CreateDefaultGradient()
    {
        Gradient g = new Gradient();
        
        // Define color keys based on your palette
        GradientColorKey[] colorKeys = new GradientColorKey[7];
        
        // 06:00 - Sabah karanlığı (00116F)
        colorKeys[0].color = new Color(0f, 0.067f, 0.435f);
        colorKeys[0].time = 0.0f;
        
        // 09:00 - Sabah aydınlığı (C3973E)
        colorKeys[1].color = new Color(0.765f, 0.592f, 0.243f);
        colorKeys[1].time = 0.125f; // (9-6)/24
        
        // 12:00 - Öğlen (D9C8B3)
        colorKeys[2].color = new Color(0.851f, 0.784f, 0.702f);
        colorKeys[2].time = 0.25f; // (12-6)/24
        
        // 15:00 - Öğleden sonra (CA8929)
        colorKeys[3].color = new Color(0.792f, 0.537f, 0.161f);
        colorKeys[3].time = 0.375f; // (15-6)/24
        
        // 18:00 - Akşam üstü kızıllığı (CC4222)
        colorKeys[4].color = new Color(0.8f, 0.259f, 0.133f);
        colorKeys[4].time = 0.5f; // (18-6)/24
        
        // 00:00 - Gece/Akşam (00116F)
        colorKeys[5].color = new Color(0f, 0.067f, 0.435f);
        colorKeys[5].time = 0.75f; // (24-6)/24
        
        // 06:00 - Sabah karanlığı tekrar (loop için)
        colorKeys[6].color = new Color(0f, 0.067f, 0.435f);
        colorKeys[6].time = 1.0f;
        
        // Alpha keys (tam opak)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;
        
        g.SetKeys(colorKeys, alphaKeys);
        return g;
    }
}
