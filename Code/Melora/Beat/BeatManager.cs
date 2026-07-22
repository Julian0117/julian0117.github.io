using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;


public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance { get; private set; }

    [Header("Tempo Settings")]
    [Range(40, 240)]
    [SerializeField] private int bpm = 60;

    // Beat Settings
    private enum BeatsPerBarOption { Four = 4, Eight = 8, Sixteen = 16 }
    [SerializeField] private BeatsPerBarOption beatsPerBarOption = BeatsPerBarOption.Four;
    public int beatsPerBar => (int)beatsPerBarOption;
    //public int beatsPerBar = 8;
    private int beatCount;

    // Bar Settings
    private enum BarsPerPhraseOption { One = 1, Two = 2, Three = 3, Four = 4 }
    [SerializeField] private BarsPerPhraseOption barsPerPhraseOption = BarsPerPhraseOption.One;
    public int barsPerPhrase => (int)barsPerPhraseOption;
    private int barCount;
    
    // Events to synchronize BeatSound objects
    public event Action<int, int> OnBeat;
    public event Action OnBar;
    public event Action<int> OnBeatsPerBarChanged;
    public event Action<BeatSound> OnBeatSoundPlayed;

    public double secondsPerBeat;
    private double nextBeatTime;
    private List<double> activeWindows = new List<double>();
    [SerializeField] private float toleranceSeconds = 0.1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        secondsPerBeat = 60.0 / bpm;
        nextBeatTime = Time.time + secondsPerBeat;
    }

    void Update()
    {
        secondsPerBeat = 60.0 / bpm * (4.0 / beatsPerBar);

        if (Time.time >= nextBeatTime)
        {
            beatCount++;

            OnBeat?.Invoke(beatCount, barCount);
            //Debug.Log($"BeatManager beatCount: {beatCount}");

            if (beatCount > beatsPerBar-1)
            {
                OnBar?.Invoke();
                beatCount = 0;
                barCount++;
                if (barCount > barsPerPhrase-1) barCount = 0;
            }

            nextBeatTime += secondsPerBeat;
        }
        activeWindows.RemoveAll(t => Time.time > t + toleranceSeconds);
    }

    private void OnValidate()
    {
        OnBeatsPerBarChanged?.Invoke(beatsPerBar); 
    }

    public void RegisterSoundWindow(double beatTime)
    {
        activeWindows.Add(beatTime);
    }

    // Checks whether an action has been performed "On Beat"
    public bool IsOnBeat()
    {
        float time = Time.time;
        foreach (double window in activeWindows)
        {
            double diff = window - time;
            double absDiff = Math.Abs(diff);

            if (diff > 0) Debug.Log($"Eingabe war: {diff} Sekunden **vor** dem Beat");

            if (diff < 0) Debug.Log($"Eingabe war: {diff} Sekunden **nach** dem Beat");

            if (absDiff <= (toleranceSeconds))
                return true;
        }
        return false;
    }

    /*public bool IsOnBeat(float toleranceSeconds = 0.1f)
    {
        float time = Time.time;

        // Calculate distance between last and next beat
        float timeSinceLastBeat = time - (nextBeatTime - secondsPerBeat);
        float timeUntilNextBeat = nextBeatTime - time;

        // If within tolerance, return true
        if (Mathf.Abs(timeSinceLastBeat) <= toleranceSeconds || Mathf.Abs(timeUntilNextBeat) <= toleranceSeconds)
            return true;

        return false;
    }*/

    public void NotifyBeatSoundPlayed(BeatSound source)
    {
        OnBeatSoundPlayed?.Invoke(source);
        //Debug.Log("OnBeatSoundPlayed invoked by " +  source);
    }
}