using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BeatFirefly : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashIntensity = 50f;
    [SerializeField] private float flashRadius = 1f;
    [SerializeField] private float fadeSpeed = 10f;
    [SerializeField] private float flashDelay = 0.05f;

    private Light2D firefly;

    private float baseIntensity;
    private float baseRadius;

    private float currentIntensity;
    private float currentRadius;

    void Start()
    {
        firefly = GetComponent<Light2D>();

        baseIntensity = firefly.intensity;
        baseRadius = firefly.pointLightOuterRadius;

        currentIntensity = baseIntensity;
        currentRadius = baseRadius;
    }

    private void Update()
    {
        // Gradual return to base values
        if (currentIntensity != baseIntensity)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, baseIntensity, fadeSpeed * Time.deltaTime);
            firefly.intensity = currentIntensity;
        }

        if (currentRadius != baseRadius)
        {
            currentRadius = Mathf.Lerp(currentRadius, baseRadius, fadeSpeed * Time.deltaTime);
            firefly.pointLightOuterRadius = currentRadius;
        }
    }

    public void Flash()
    {
        StopCoroutine(nameof(FlashRoutine));
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        // Short delay to better sync with sound
        if (flashDelay > 0f)
            yield return new WaitForSeconds(flashDelay);

        currentIntensity = flashIntensity;
        currentRadius = flashRadius;

        firefly.intensity = currentIntensity;
        firefly.pointLightOuterRadius = currentRadius;
    }
}