using UnityEngine;
using System.Collections.Generic;

public class AppleGroupTrigger : MonoBehaviour
{
    [SerializeField] private int beatsPerApple = 4;

    private List<Rigidbody2D> apples = new List<Rigidbody2D>();
    private int rabbitThumps = 0;
    private int currentAppleIndex = 0;

    private void Awake()
    {
        apples.Clear();

        foreach (Transform child in transform)
        {
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                apples.Add(rb);
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    private void Start()
    {
        BeatManager.Instance.OnBeatSoundPlayed += HandleBeat;
    }

    private void OnDestroy()
    {
        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeatSoundPlayed -= HandleBeat;
    }

    private void HandleBeat(BeatSound source)
    {
        if (!source.CompareTag("Rabbit"))
            return;

        rabbitThumps++;

        if (rabbitThumps >= beatsPerApple)
        {
            rabbitThumps = 0;
            ActivateNextApple();
        }
    }

    private void ActivateNextApple()
    {
        if (currentAppleIndex >= apples.Count)
            return;

        apples[currentAppleIndex].bodyType = RigidbodyType2D.Dynamic;
        currentAppleIndex++;
    }
}