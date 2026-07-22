using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightingController : MonoBehaviour
{
    private Light2D globalLight;

    [SerializeField] private Light2D sunLight;
    [SerializeField] private float fadeInDuration = 8f;

    [Header("Flash Einstellungen")]
    [SerializeField] private float flashIntensity = 1.5f; 
    [SerializeField] private float flashDuration = 0.2f;  
    [SerializeField] private float sunIntensityIncrease = 0.1f; 
    [SerializeField] private float globalIntensityIncrease = 0.1f; 

    [SerializeField] private Color flashColor = Color.green;

    private float globalTargetIntensity;
    private float sunTargetIntensity;
    private Color sunOriginalColor;

    private void Awake()
    {
        if (globalLight == null)
            globalLight = GetComponent<Light2D>();
        
        globalTargetIntensity =  globalLight.intensity;

        if (sunLight != null)
        {
            sunTargetIntensity = sunLight.intensity;
            sunOriginalColor = sunLight.color; // Farbe sichern
        }
    }

    private void Start()
    {
        if (globalLight == null) return;

        globalLight.intensity = 0f;
        if (sunLight != null)
        {
            sunLight.intensity = 0f;
            StartCoroutine(FadeInSun());
        }
        StartCoroutine(FadeInLight());
    }

    public void SunFlash()
    {
        if (globalLight.intensity + globalIntensityIncrease <= 1f) {
            globalLight.intensity += globalIntensityIncrease;
        }
        else
        {
            globalLight.intensity = 1f;
        }
        if (sunLight != null) StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float startIntensity = sunLight.intensity;
        float peakIntensity = startIntensity + flashIntensity;
        sunTargetIntensity += sunIntensityIncrease; 

        float elapsed = 0f;
        float peakDuration = flashDuration * 0.1f;
        while (elapsed < peakDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / peakDuration;
            
            sunLight.intensity = Mathf.Lerp(startIntensity, peakIntensity, t);
            sunLight.color = Color.Lerp(sunOriginalColor, flashColor, t);
            yield return null;
        }

        elapsed = 0f;
        float settleDuration = flashDuration * 0.9f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / settleDuration);
            
            sunLight.intensity = Mathf.Lerp(peakIntensity, sunTargetIntensity, t);
            sunLight.color = Color.Lerp(flashColor, sunOriginalColor, t);
            yield return null;
        }

        sunLight.intensity = sunTargetIntensity;
        sunLight.color = sunOriginalColor;
    }
    
    private IEnumerator FadeInLight()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            globalLight.intensity = Mathf.Pow(t, 3f) * globalTargetIntensity;
        
            yield return null;
        }

        globalLight.intensity = globalTargetIntensity;
    }

    private IEnumerator FadeInSun()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            sunLight.intensity = Mathf.Lerp(0f, sunTargetIntensity, Mathf.Pow(t, 3f));
            yield return null;
        }
        sunLight.intensity = sunTargetIntensity;
    }
}