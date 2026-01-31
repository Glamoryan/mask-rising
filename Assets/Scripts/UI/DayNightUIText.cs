using TMPro;
using UnityEngine;

public class DayNightUIText : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
        if (!cycle) cycle = FindFirstObjectByType<DayNightCycle>();
    }

    private void OnEnable()
    {
        if (!cycle) return;
        cycle.OnDayStart += HandleDayStart;
        cycle.OnNightStart += HandleNightStart;
    }

    private void OnDisable()
    {
        if (!cycle) return;
        cycle.OnDayStart -= HandleDayStart;
        cycle.OnNightStart -= HandleNightStart;
    }

    private void Start()
    {
        UpdateText(cycle.Phase);
    }

    private void HandleDayStart(int _day)
    {
        UpdateText(DayPhase.Day);
    }

    private void HandleNightStart(int _day)
    {
        UpdateText(DayPhase.Night);
    }

    private void UpdateText(DayPhase phase)
    {
        if (phase == DayPhase.Day)
        {
            label.text = "DAY";
            label.color = new Color(1f, 0.9f, 0.6f); // warm
        }
        else
        {
            label.text = "NIGHT";
            label.color = new Color(0.6f, 0.8f, 1f); // cold
        }
    }
}
