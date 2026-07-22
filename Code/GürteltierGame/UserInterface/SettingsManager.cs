using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    [Header("Brightness Settings")]
    public Volume volume;
    public Slider brightnessSlider;

    private ColorAdjustments colorAdjustments;

    private void Start()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);

        if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);

            float normalizedValue = Mathf.InverseLerp(-2f, 2f, colorAdjustments.postExposure.value);
            brightnessSlider.value = normalizedValue;
        }
        else
        {
            Debug.LogError("ColorAdjustments nicht im Volume-Profil gefunden.");
        }
    }

    public void SetVolume(float value)
    {
        // Wert in dB umrechnen (AudioMixer erwartet dB)
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);
    }

    public void SetBrightness(float value)
    {
        if (colorAdjustments != null)
        {
            // Skaliere Slider [0–1] auf Exposure [-2, +2]
            colorAdjustments.postExposure.value = Mathf.Lerp(-2f, 2f, value);
        }
    }
}