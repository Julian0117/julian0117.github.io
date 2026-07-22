using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FlyGlide : MonoBehaviour
{
    [Header("Gleitflug-Einstellungen")]
    public float flightTime = 1.5f;
    public float arcHeight = 1.0f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasHit = false; // damit nur 1x reagiert wird

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void StartGlide(Vector3 start, Vector3 end)
    {
        StopAllCoroutines();
        StartCoroutine(GlideRoutine(start, end));
    }

    private IEnumerator GlideRoutine(Vector3 start, Vector3 end)
    {
        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        float t = 0f;
        float stepTime = Time.fixedDeltaTime;

        while (t < 1f && !hasHit)
        {
            t += stepTime / flightTime;

            Vector3 nextPos = Vector3.Lerp(start, end, t);
            nextPos.y += Mathf.Sin(Mathf.PI * t) * arcHeight;

            rb.MovePosition(nextPos);
            Physics2D.SyncTransforms();

            yield return new WaitForFixedUpdate();
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
    }

    //  Wird direkt vom Unity-Physiksystem ausgelöst — absolut „instant“
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Frog")) // Frosch-Tag prüfen
        {
            hasHit = true;
            Debug.Log("Fliege berührt Frosch → sofort zerstört!");
            Destroy(gameObject);
        }
    }
}
