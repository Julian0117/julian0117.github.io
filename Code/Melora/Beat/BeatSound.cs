using System;
using UnityEngine;

public class BeatSound : Interactable
{
    protected BeatManager beatManager;
    [SerializeField] protected UIManager uiManager;
    private bool uiShowing = false;

    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private TutorialVideo tutorialVideo;

    [SerializeField] private AlarmClock alarmClock;

    [SerializeField] private LightingController lightingController;
    
    private AudioSource audioSource;
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] protected AudioClip audioClip;
    [SerializeField] protected AudioClip fanfare;
    [SerializeField] protected AudioClip toggleOnSound;
    [SerializeField] protected AudioClip toggleOffSound;
    [SerializeField] private int totalBeatCount = 0;
    private bool firstEnabled = false;
    public bool isPlaying = true;

    [Header("PlayOnBeat...?")]
    [SerializeField] private bool[] beats;
    private GameObject beatUI;
    private BeatUI beatUIController;

    //[SerializeField] private string animationTrigger = "Play";
    private Animator[] anims;

    private void Start()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        anims = GetComponentsInChildren<Animator>();
        
        beatUI = GetComponentInChildren<Canvas>().gameObject;
        beatUIController = beatUI.GetComponent<BeatUI>();
        beatUI.SetActive(false);
        
        beatManager = BeatManager.Instance;

        beatManager.OnBeat += HandleBeat;
        int beatsPerBar = beatManager.beatsPerBar; 
        SetBeats(beatsPerBar);
        //beatManager.OnBeatsPerBarChanged += ChangeBeatsPerBar;
        beatManager.OnBeatsPerBarChanged += SetBeats;
    }

    public void FirstEnable()
    {
        Debug.Log($"{this} FirstEnable() called!");
        if (firstEnabled) return;
        firstEnabled = true;
        uiAudioSource.PlayOneShot(fanfare);
        lightingController.SunFlash();
        if (tutorialPopup != null) tutorialPopup.SetActive(true);
        if (alarmClock != null) alarmClock.CountBeatSound();
        isPlaying = true;
        //beats[0] = true;
        beatUIController.RebuildUI(0);
        //beatUI.SetActive(true);
        //ShowBeatUI(true);
        if (anims != null)
        {
            foreach (Animator anim in anims)
            {
                anim.SetTrigger("Activate");
            }
        }
    }

    /*private void ShowBeatUI(bool show)
    {
        beatUI.SetActive(show);
        uiManager.SetCursorState(show);
        uiShowing = show;
    }*/

    public override void Interact(PlayerController player)
    {
        //if (!isPlaying) return;
        beatUI.SetActive(!beatUI.activeSelf);
        //ShowBeatUI(!uiShowing);
        
    }

    private void SetBeats(int newBeatsPerBar)
    {
        bool[] oldBeats = beats;
        beats = new bool[newBeatsPerBar];

        if (oldBeats == null || oldBeats.Length == 0)
            return;

        int oldBeatsPerBar = oldBeats.Length;

        // Nur erlaubte Längen
        if ((oldBeatsPerBar != 4 && oldBeatsPerBar != 8 && oldBeatsPerBar != 16) ||
            (newBeatsPerBar != 4 && newBeatsPerBar != 8 && newBeatsPerBar != 16))
        {
            return;
        }

        if (newBeatsPerBar >= oldBeatsPerBar)
        {
            // Hochskalieren: alte Werte gleichmäßig verteilen, dazwischen false
            float step = (float)newBeatsPerBar / oldBeatsPerBar;

            for (int oldIndex = 0; oldIndex < oldBeatsPerBar; oldIndex++)
            {
                int newIndex = Mathf.RoundToInt(oldIndex * step);
                if (newIndex >= newBeatsPerBar) newIndex = newBeatsPerBar - 1;

                beats[newIndex] = oldBeats[oldIndex];
            }
            // Alle anderen neuen Plätze bleiben false
        }
        else
        {
            // Runterskalieren: ähnlich wie bisher Floor-Mapping
            float ratio = (float)oldBeatsPerBar / newBeatsPerBar;

            for (int newIndex = 0; newIndex < newBeatsPerBar; newIndex++)
            {
                int oldIndex = Mathf.FloorToInt(newIndex * ratio);
                if (oldIndex < 0 || oldIndex >= oldBeatsPerBar)
                    continue;

                beats[newIndex] = oldBeats[oldIndex];
            }
        }
        beatUIController.RebuildUI(newBeatsPerBar);
    }
    
    private void ChangeBeatsPerBar(int newBeatsPerBar)
    {
        SetBeats(newBeatsPerBar);
    }

    /*
    public void StartPlaying()
    {
        isPlaying = true;
    }

    public void StopPlaying()
    {
        isPlaying = false;
    }*/

    public bool GetBeat(int index)
    {
        if (beats == null || index < 0 || index >= beats.Length) return false;

        return beats[index];
    }
    
    public void SetBeat(int beatIndex, bool value)
    {
        if (tutorialVideo != null) tutorialVideo.Stop();
        if (value)
        {
            uiAudioSource.PlayOneShot(toggleOnSound);
        }
        else
        {
            uiAudioSource.PlayOneShot(toggleOffSound);
        }
        beats[beatIndex] = value;
        Debug.Log($"Beat {beatIndex} = {value}");
    }

    private void HandleBeat(int beatIndex, int barIndex)
    {
        if (beatIndex > beats.Length || beatIndex < 0) return;
        
        totalBeatCount++;
        if (!isPlaying) return;

        int current = beatIndex - 1;
        int next = (current + 1) % beats.Length;

        //Debug.Log($"Current beatIndex: {current}");
        bool playSound = beats[current];

        // If next beat is set to play, add its window to BeatManager
        if (beats[next])
        {
            double nextBeatTime = Time.time + BeatManager.Instance.secondsPerBeat;
            BeatManager.Instance.RegisterSoundWindow(nextBeatTime);
        }

        // Play sound on current beat
        if (playSound)
        {
            Play();
        }
    }

    protected virtual void Play()
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
        if (anims != null)
        {
            foreach (Animator anim in anims) {
                anim.SetTrigger("Play");
            }
        }
        beatManager.NotifyBeatSoundPlayed(this);
    }
}