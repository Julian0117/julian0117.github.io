using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Komponenten")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public CanvasGroup menuCanvasGroup;
    public Camera mainCamera;
    public CanvasGroup fadeGroup;
    public string nextSceneName;

    [Header("Einstellungen")]
    public List<VideoClip> introVideos;
    public float fadeDuration = 1.0f;
    public Color targetBackgroundColor = Color.black;
    [Tooltip("Zeit in Sekunden, bis das nächste Video automatisch startet, wenn keine Eingabe erfolgt.")]
    public float nextVideoWaitTime = 5.0f;

    private int currentVideoIndex = 0;
    private bool anyKeyPressed = false;

    public void StartIntro()
    {
        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        // Fade für Menü starten
        StartCoroutine(FadeMenu(menuCanvasGroup.alpha, 0f, fadeDuration - 1));
        yield return StartCoroutine(FadeBackground(mainCamera.backgroundColor, targetBackgroundColor, fadeDuration));

        menuCanvasGroup.gameObject.SetActive(false);

        yield return StartCoroutine(FadeSprite(0, 1, fadeDuration - 1));
        yield return StartCoroutine(FadeSprite(1, 0, fadeDuration - 1));

        while (currentVideoIndex < introVideos.Count)
        {
            videoPlayer.clip = introVideos[currentVideoIndex];
            videoPlayer.Prepare();
            
            while (!videoPlayer.isPrepared) yield return null;

            fadeGroup.alpha = 0; 
            videoPlayer.Play();

            while (videoPlayer.isPlaying || videoPlayer.frame < (long)videoPlayer.frameCount - 1)
            {
                if (videoPlayer.frame >= (long)videoPlayer.frameCount - 2)
                {
                    videoPlayer.Pause();
                    break;
                }
                yield return null;
            }

            anyKeyPressed = false;
            float timer = nextVideoWaitTime;

            using (var subscription = InputSystem.onAnyButtonPress.Call(_ => anyKeyPressed = true))
            {
                while (!anyKeyPressed && timer > 0)
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }
            }
            // ------------------------------------------------

            currentVideoIndex++;
        }

        StartCoroutine(FadeVideo(1, 0, fadeDuration));
        yield return StartCoroutine(FadeAudio(0.5f, 0, fadeDuration));
        
        SceneManager.LoadScene(nextSceneName);
    }

    // --- Hilfsmethoden ---

    IEnumerator FadeSprite(float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            fadeGroup.alpha = Mathf.Lerp(start, end, smoothT);
            yield return null;
        }
        fadeGroup.alpha = end;
    }
    
    IEnumerator FadeVideo(float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            videoPlayer.targetCameraAlpha = Mathf.Lerp(start, end, smoothT);
            yield return null;
        }
        videoPlayer.targetCameraAlpha = end;
    }
    
    IEnumerator FadeAudio(float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            audioSource.volume = Mathf.Lerp(start, end, smoothT);
            yield return null;
        }
        audioSource.volume = end;
    }
    
    IEnumerator FadeMenu(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            menuCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);
            yield return null;
        }
        menuCanvasGroup.alpha = endAlpha;
    }

    IEnumerator FadeBackground(Color startColor, Color endColor, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            mainCamera.backgroundColor = Color.Lerp(startColor, endColor, smoothT);
            yield return null;
        }
        mainCamera.backgroundColor = endColor;
    }
}