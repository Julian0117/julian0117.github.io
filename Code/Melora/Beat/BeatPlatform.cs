using UnityEngine;

public class BeatPlatform : MonoBehaviour
{
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float pulseDuration = 0.25f;
    [SerializeField] private float frequency = 2f;

    [SerializeField] private float pulseSetting = 0f;
    private float pulseStrength = 0f;
    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }

    private void Start()
    {
        BeatManager.Instance.OnBeatSoundPlayed += OnBeat;
    }

    /*private void OnDisable()
    {
        BeatManager.Instance.OnBeatSoundPlayed -= OnBeat;
    }*/

    private void OnBeat(BeatSound sound)
    {
        Debug.Log($"BeatPlatform: {this.name} OnBeat() called by {sound}");
        // Jeder BeatSound gibt der Plattform einen Impuls
        pulseStrength = pulseSetting;
    }

    private void Update()
    {
        if (pulseStrength > 0f)
        {
            float t = Time.time * frequency;
            float movement = Mathf.Sin(t * Mathf.PI * 2f) * amplitude * pulseStrength;

            transform.position = startPos + new Vector3(0f, movement, 0f);

            // Impuls linear abbauen
            pulseStrength -= Time.deltaTime / pulseDuration;
            pulseStrength = Mathf.Clamp01(pulseStrength);
        }
    }
}