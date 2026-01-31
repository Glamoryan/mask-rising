using System;
using UnityEngine;

public enum DayPhase { Day, Night }

public class DayNightCycle : MonoBehaviour
{
    [Header("Timing (seconds)")]
    [Min(1f)] public float dayDuration = 60f;   // 12 hours x 5 seconds = 60 seconds
    [Min(1f)] public float nightDuration = 60f; // 12 hours x 5 seconds = 60 seconds
    [Tooltip("Real seconds per game hour")]
    public float secondsPerHour = 5f;

    [Header("Starting Phase")]
    [SerializeField] private DayPhase startingPhase = DayPhase.Day;
    [SerializeField] private int startingDayIndex = 1;

    [Header("State (read-only)")]
    [SerializeField] private DayPhase phase = DayPhase.Day;
    [SerializeField] private int dayIndex = 1; // 1-based

    // Normalized progress within current phase (0..1)
    public float PhaseT => Mathf.Clamp01(phaseTimer / CurrentPhaseDuration);

    public DayPhase Phase => phase;
    public int DayIndex => dayIndex;
    public float TimeLeft => Mathf.Max(0f, CurrentPhaseDuration - phaseTimer);
    
    // Time of day (0-24 hours)
    public float CurrentHour
    {
        get
        {
            if (phase == DayPhase.Day)
            {
                // Day: 6:00 AM to 6:00 PM (6.0 to 18.0)
                return 6f + (PhaseT * 12f);
            }
            else
            {
                // Night: 6:00 PM to 6:00 AM (18.0 to 30.0, wrapped to 0-6)
                float hour = 18f + (PhaseT * 12f);
                if (hour >= 24f) hour -= 24f;
                return hour;
            }
        }
    }
    
    // Formatted time string (HH:MM)
    public string CurrentTimeString
    {
        get
        {
            int hours = Mathf.FloorToInt(CurrentHour);
            int minutes = Mathf.FloorToInt((CurrentHour - hours) * 60f);
            return $"{hours:D2}:{minutes:D2}";
        }
    }

    public event Action<int> OnDayStart;     // dayIndex
    public event Action<int> OnNightStart;   // dayIndex
    public event Action<DayPhase, float> OnTick; // phase, normalized t

    private float phaseTimer;

    private float CurrentPhaseDuration => phase == DayPhase.Day ? dayDuration : nightDuration;

    private void Awake()
    {
        // Initialize phase early so others can read it
        phase = startingPhase;
        dayIndex = startingDayIndex;
        phaseTimer = 0f;
    }

    private void Start()
    {
        // Fire initial phase start events so UI/visuals initialize correctly
        if (phase == DayPhase.Day) 
            OnDayStart?.Invoke(dayIndex);
        else 
            OnNightStart?.Invoke(dayIndex);
        
        // Fire initial tick
        OnTick?.Invoke(phase, 0f);
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
