using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("References")]
    public Transform visual;  // Child mit SpriteRenderer

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private BoxCollider2D col;

    private bool isOpen = false;
    private bool initialized = false;

    private void Awake()
    {
        sr = visual.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<BoxCollider2D>();

        ApplyState(playSound: false);
        initialized = true;
    }

    public void Toggle(Vector2 playerPos)
    {
        // Öffnungsrichtung anhand Spielerposition
        bool playerIsRight = playerPos.x > transform.position.x;

        // Flip für richtige Richtung:
        sr.flipX = playerIsRight;   // Spieler rechts = Tür schwingt nach rechts

        isOpen = !isOpen;
        ApplyState(true);
    }

    private void ApplyState(bool playSound)
    {
        // Sprite setzen
        sr.sprite = isOpen ? openSprite : closedSprite;

        // Collider (im geöffneten Zustand deaktiviert)
        col.enabled = !isOpen;

        // Sound
        if (playSound && initialized && audioSource != null)
        {
            if (isOpen)
                audioSource.PlayOneShot(openSound);
            else
                audioSource.PlayOneShot(closeSound);
        }
    }
}

