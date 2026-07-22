using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class TutorialVideo : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplayImage;
    private Coroutine currentFadeRoutine;
    private bool firstTime = true;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        videoPlayer.targetCameraAlpha = 0f; 
    }
    
    public void Play()
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        
        currentFadeRoutine = StartCoroutine(PlayRoutine());
    }

    public void Stop()
    {
        if (!firstTime) return;
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(StopRoutine());
        firstTime = false;
    }
    
    private IEnumerator PlayRoutine()
    {
        videoPlayer.Play();
        yield return StartCoroutine(FadeVideo(0f, 1f, 0.5f));
    }

    private IEnumerator StopRoutine()
    {
        yield return StartCoroutine(FadeVideo(1f, 0f, 0.5f));
        videoPlayer.Stop();

    }
    
    private IEnumerator FadeVideo(float start, float end, float duration)
    {
        float elapsed = 0;
    
        Color tempColor = videoDisplayImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
        
            tempColor.a = Mathf.Lerp(start, end, smoothT);
        
            videoDisplayImage.color = tempColor;
        
            yield return null;
        }
    
        tempColor.a = end;
        videoDisplayImage.color = tempColor;
    }
}