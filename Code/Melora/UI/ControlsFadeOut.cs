using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ControlsFadeOut : MonoBehaviour
{
    [Header("Einstellungen")]
    [Tooltip("Die Taste, die den Fade auslöst")]
    [SerializeField] private Key triggerKey = Key.D;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private TutorialVideo tutorialVideo;

    private SpriteRenderer[] childSprites;
    private bool isFading = false;

    void Start()
    {
        childSprites = GetComponentsInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current[triggerKey].isPressed && !isFading)
            {
                StartCoroutine(FadeAllChildren());
            }
        }
    }

    private IEnumerator FadeAllChildren()
    {
        isFading = true;
        float elapsed = 0f;

        float startAlpha = 1f; 

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / fadeDuration);
            float currentAlpha = Mathf.Lerp(startAlpha, 0f, t);

            foreach (SpriteRenderer sprite in childSprites)
            {
                if (sprite != null)
                {
                    Color c = sprite.color;
                    c.a = currentAlpha;
                    sprite.color = c;
                }
            }
            yield return null;
        }

        foreach (SpriteRenderer sprite in childSprites)
        {
            if (sprite != null)
            {
                Color c = sprite.color;
                c.a = 0f;
                sprite.color = c;
            }
        }

        if (tutorialVideo != null) tutorialVideo.Play();
    }
}