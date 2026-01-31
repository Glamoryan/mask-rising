using TMPro;
using UnityEngine;

public class DayNightUIText : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private TMP_Text label;

    [Header("Colors")]
    [SerializeField] private Color dayTextColor = new Color(1f, 0.9f, 0.6f);
    [SerializeField] private Color nightTextColor = new Color(0.6f, 0.8f, 1f);

    private void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
        if (!cycle) cycle = FindFirstObjectByType<DayNightCycle>();
    }

    private void Update()
    {
        if (!cycle || !label) return;

        // Update time display
        label.text = cycle.CurrentTimeString;
        
        // Update color based on phase
        label.color = (cycle.Phase == DayPhase.Day) ? dayTextColor : nightTextColor;
    }
}
