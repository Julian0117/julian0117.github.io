using UnityEngine;

public class Konfetti : MonoBehaviour
{
    ParticleSystem confettiCanon;
    SpriteRenderer spriteRenderer;
    Color baseColor;
    [SerializeField] Color glowColor = Color.white;
    [SerializeField] float glowDuration = 0.2f;

    void Start()
    {
        confettiCanon = GetComponentInChildren<ParticleSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.material.GetColor("_Color");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"Trigger entered by {col.name}");
        FireConfetti();
        StartCoroutine(GlowEffect());
    }

    private void FireConfetti()
    {
        if (confettiCanon != null)
            confettiCanon.Play();
    }

    private System.Collections.IEnumerator GlowEffect()
    {
        // Aufleuchten
        spriteRenderer.material.SetColor("_Color", glowColor);

        // Wenn dein Material einen Emission-Parameter hat:
        if (spriteRenderer.material.HasProperty("_EmissionColor"))
            spriteRenderer.material.SetColor("_EmissionColor", glowColor * 2f);

        yield return new WaitForSeconds(glowDuration);

        // Zurück zur ursprünglichen Farbe
        spriteRenderer.material.SetColor("_Color", baseColor);
        if (spriteRenderer.material.HasProperty("_EmissionColor"))
            spriteRenderer.material.SetColor("_EmissionColor", Color.black);
    }
}