using UnityEngine;

public class WorldTint : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.65f, 0.7f, 1f, 1f);

    [Tooltip("Assign a root that contains ONLY world SpriteRenderers (exclude UI). Optional.")]
    [SerializeField] private Transform worldRoot;

    private SpriteRenderer[] renderers;

    private void Awake()
    {
        if (!cycle) cycle = FindFirstObjectByType<DayNightCycle>();

        renderers = worldRoot
            ? worldRoot.GetComponentsInChildren<SpriteRenderer>(true)
            : FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        if (!cycle) return;
        cycle.OnTick += HandleTick;
    }

    private void OnDisable()
    {
        if (!cycle) return;
        cycle.OnTick -= HandleTick;
    }

    private void HandleTick(DayPhase phase, float t)
    {
        // Smooth blend throughout the phase (simple + stable)
        Color c = (phase == DayPhase.Day)
            ? Color.Lerp(nightColor, dayColor, t)
            : Color.Lerp(dayColor, nightColor, t);

        ApplyColor(c);
    }

    private void ApplyColor(Color c)
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = c;
    }
}
