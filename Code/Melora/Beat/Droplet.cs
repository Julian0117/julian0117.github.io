using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class Droplet : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;
    private AudioSource audioSource;
    public AudioClip audioClip;

    [SerializeField] private string splashTrigger = "Play";
    //[SerializeField] private float fallSpeed = -4f;

    private Vector3 baseScale;
    private Transform spawnPoint;
    private System.Action<Droplet> onPlayed;

    private bool hasSplashed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        baseScale = transform.localScale;
        gameObject.SetActive(false);
    }

    public void Init(Transform spawnPoint, System.Action<Droplet> playedCallback)
    {
        this.spawnPoint = spawnPoint;
        this.onPlayed = playedCallback;
    }

    public void Spawn()
    {
        transform.position = spawnPoint.position;

        // leichte zufällige Rotation
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));

        // kleine scale variation
        float scaleVar = Random.Range(0.9f, 1.1f);
        transform.localScale = baseScale * scaleVar;

        // 50% Mirror
        if (Random.value < 0.5f)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        //rb.linearVelocity = new Vector2(0, fallSpeed);
        rb.simulated = true;

        gameObject.SetActive(true);
        col.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"{this} OnCollisionEnter2D() called. Collided with {collision.transform.gameObject.name}");
        // Kollision mit anderen Droplets ignorieren
        if (collision.gameObject.layer == LayerMask.NameToLayer("Droplets"))
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        animator.SetTrigger(splashTrigger);
        audioSource.PlayOneShot(audioClip);
        onPlayed?.Invoke(this);
    }

    // Wird vom letzten Frame der Splash-Animation als AnimationEvent aufgerufen
    public void OnSplashFinished()
    {
        //Debug.Log($"{this} OnSplashFinished() called");
        ResetDroplet();
    }

    private void ResetDroplet()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint.position;
        gameObject.SetActive(false);
    }
}