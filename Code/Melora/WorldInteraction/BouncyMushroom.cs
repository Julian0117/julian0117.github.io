using UnityEngine;

public class BouncyMushroom : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 25f;         // Stärke des Sprungs
    [SerializeField] private bool bounceOnlyFromTop = true;   // nur von oben triggern
    [SerializeField] private float disableTime = 0.3f;        // kurze Pause, bis Bounce wieder aktiv wird

    private bool hasBounced = false;   // merkt sich, ob gerade gebounct wurde
    private float lastBounceTime = 0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // prüfen, ob der Player von oben kommt
        bool validHit = true;
        if (bounceOnlyFromTop && collision.contacts.Length > 0)
        {
            ContactPoint2D contact = collision.contacts[0];
            validHit = Vector2.Dot(contact.normal, Vector2.up) > 0.5f;
        }

        // Wenn gültiger Aufprall, kein Dauertrigger, und Pause abgelaufen
        if (validHit && !hasBounced && Time.time - lastBounceTime > disableTime)
        {
            PerformBounce(rb);
        }
    }

    private void PerformBounce(Rigidbody2D rb)
    {
        // Alte Vertikalgeschwindigkeit nullen für gleichmäßigen Effekt
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Einmaliger Impuls nach oben
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        Debug.Log("Einmaliger Bounce ausgelöst!");

        // Flags setzen, damit nicht dauerhaft gebounct wird
        hasBounced = true;
        lastBounceTime = Time.time;

        // Nach kurzer Zeit wieder aktivierbar (wenn Player neu landet)
        Invoke(nameof(ResetBounce), disableTime);
    }

    private void ResetBounce()
    {
        hasBounced = false;
    }
}

