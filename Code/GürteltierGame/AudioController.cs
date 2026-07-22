using UnityEngine;


public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    public AudioSource speaker;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private float bounceVolume = 1.0f;

    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private float jumpVolume = 1.0f;

    [SerializeField] private AudioClip dashSound;
    [SerializeField] private float dashVolume = 1.0f;

    [SerializeField] private AudioClip gravityInvSoundOne;
    [SerializeField] private AudioClip gravityInvSoundTwo;
    [SerializeField] private float gravityInvVolume = 1.0f;
    [SerializeField] private AudioClip magnetOnSound;
    [SerializeField] private AudioClip magnetOffSound;
    private float magnetVolume = 1.0f;

    [SerializeField] private AudioClip starSound;
    [SerializeField] private float starVolume = 1.0f;

    [SerializeField] private AudioClip[] fruitSounds;
    [SerializeField] private float fruitVolume = 1.0f;

    [SerializeField] private AudioClip powerupSound;
    [SerializeField] private float powerupVolume = 1.0f;


    [Header("Surface Sounds")]
    [SerializeField] private AudioClip dirtSound;
    [SerializeField] private AudioClip grassSound;
    [SerializeField] private AudioClip sandSound;
    [SerializeField] private AudioClip stoneSound;
    [SerializeField] private float surfaceVolume = 1.0f;


    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float bgmVolume = 1.0f;
    [SerializeField] private bool playBGM = false;
    private bool bgmIsPlaying = false;

    private bool gravitySwitched = false;

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
        //Play(backgroundMusic);
    }

    private void FixedUpdate()
    {
        if (playBGM && !bgmIsPlaying)
        {
            PlayMusic(backgroundMusic, bgmVolume);
        } 
        else if (!playBGM && bgmIsPlaying)
        {
            StopMusic();
        }
    }

    public void PlayBounceSound(Vector3 position)
    {
        Play(bounceSound, position, bounceVolume); 
    }

    public void PlayJumpSound()
    {
        PlayRandom(jumpSounds, jumpVolume);
    }

    public void PlayDashSound()
    {
        Play(dashSound, dashVolume);
    }

    public void PlayDirtSound()
    {
        Play(dirtSound, surfaceVolume);
    }
    public void PlayGrassSound()
    {
        Play(grassSound, surfaceVolume);
    }

    public void PlaySandSound()
    {
        Play(sandSound, surfaceVolume);   
    }

    public void PlayStoneSound()
    {
        Play(stoneSound, surfaceVolume);
    }

    public void PlayStarSound()
    {
        Play(starSound, starVolume);
    }

    public void PlayPowerupSound()
    {
        Play(powerupSound, powerupVolume);
    }

    public void PlayFruitSound()
    {
        PlayRandom(fruitSounds, fruitVolume);
    }

    public void PlayGravitySound()
    {
        if (!gravitySwitched)
        {
            Play(gravityInvSoundOne, gravityInvVolume);
            gravitySwitched = true;
        }
        else if (gravitySwitched)
        {
            Play(gravityInvSoundTwo, gravityInvVolume);
            gravitySwitched = false;
        }
    }

    public void PlayMagnetToggleSound(bool on)
    {
        if (on)
        {
            Play(magnetOnSound, magnetVolume);    
        }
        else
        {
            Play(magnetOffSound, magnetVolume);
        }
    }

    // === INTERNE PLAY METHODE ===

    private void Play(AudioClip clip, float volume)
    {
        if (clip != null)
            speaker.PlayOneShot(clip, volume);
    }

    private void Play(AudioClip clip, Vector3 position, float volume)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    private void PlayRandom(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0)
            return;

        int index = Random.Range(0, clips.Length);
        Play(clips[index], volume);
    }

    private void PlayMusic(AudioClip music, float volume)
    {
        if (music != null)
        {
            speaker.clip = music;
            speaker.loop = true;
            speaker.Play();
            bgmIsPlaying = true;
        }
    }

    private void StopMusic()
    {
        speaker.Stop();
        bgmIsPlaying = false;
    }
}