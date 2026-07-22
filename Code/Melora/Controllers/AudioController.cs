using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 8f;

    private void Start()
    {
        AudioListener.volume = 0f;
        StartCoroutine(FadeInVolume());
    }

    private IEnumerator FadeInVolume()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            AudioListener.volume = Mathf.Pow(t, 3f);
            yield return null;
        }

        AudioListener.volume = 1f;
    }
}