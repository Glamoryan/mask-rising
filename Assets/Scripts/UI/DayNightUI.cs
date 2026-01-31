using UnityEngine;
using TMPro;

public class DayNightUI : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (!cycle) cycle = FindFirstObjectByType<DayNightCycle>();
    }

    private void Update()
    {
        if (!cycle || !label) return;

        string phase = cycle.Phase == DayPhase.Day ? "DAY" : "NIGHT";
        label.text = $"Day {cycle.DayIndex} — {phase} ({Mathf.CeilToInt(cycle.TimeLeft)}s)";
    }
}
