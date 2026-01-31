using System;
using UnityEngine;

public enum DayPhase { Day, Night }

public class DayNightCycle : MonoBehaviour
{
    [Header("Timing (seconds)")]
    [Min(1f)] public float dayDuration = 45f;
    [Min(1f)] public float nightDuration = 45f;

    [Header("State (read-only)")]
    [SerializeField] private DayPhase phase = DayPhase.Day;
    [SerializeField] private int dayIndex = 1; // 1-based

    // Normalized progress within current phase (0..1)
    public float PhaseT => Mathf.Clamp01(phaseTimer / CurrentPhaseDuration);

    public DayPhase Phase => phase;
    public int DayIndex => dayIndex;
    public float TimeLeft => Mathf.Max(0f, CurrentPhaseDuration - phaseTimer);

    public event Action<int> OnDayStart;     // dayIndex
    public event Action<int> OnNightStart;   // dayIndex
    public event Action<DayPhase, float> OnTick; // phase, normalized t

    private float phaseTimer;

    private float CurrentPhaseDuration => phase == DayPhase.Day ? dayDuration : nightDuration;

    private void Start()
    {
        // Fire initial phase start events so UI/visuals initialize correctly
        if (phase == DayPhase.Day) OnDayStart?.Invoke(dayIndex);
        else OnNightStart?.Invoke(dayIndex);
    }

    private void Update()
    {
        phaseTimer += Time.deltaTime;

        OnTick?.Invoke(phase, PhaseT);

        if (phaseTimer >= CurrentPhaseDuration)
        {
            phaseTimer = 0f;
            SwitchPhase();
        }
    }

    private void SwitchPhase()
    {
        if (phase == DayPhase.Day)
        {
            phase = DayPhase.Night;
            OnNightStart?.Invoke(dayIndex);
        }
        else
        {
            phase = DayPhase.Day;
            dayIndex += 1;
            OnDayStart?.Invoke(dayIndex);
        }
    }

    // Optional: force phase for debug
    [ContextMenu("Force Day")]
    public void ForceDay()
    {
        phase = DayPhase.Day;
        phaseTimer = 0f;
        OnDayStart?.Invoke(dayIndex);
    }

    [ContextMenu("Force Night")]
    public void ForceNight()
    {
        phase = DayPhase.Night;
        phaseTimer = 0f;
        OnNightStart?.Invoke(dayIndex);
    }
}
