using UnityEngine;
using System.Collections.Generic;

public class FireflyManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject fireflyPrefab;
    [SerializeField] private int fireflyCount = 20;

    private List<BeatFirefly> fireflies = new List<BeatFirefly>();

    void Start()
    {
        SpawnFireflies();
        BeatManager.Instance.OnBeatSoundPlayed += HandleBeatSoundPlayed;
    }

    void OnDestroy()
    {
        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeatSoundPlayed -= HandleBeatSoundPlayed;
    }

    private void SpawnFireflies()
    {
        Vector2 center = transform.position;
        Vector2 size = transform.localScale;
        Vector2 half = size * 0.5f;

        for (int i = 0; i < fireflyCount; i++)
        {
            // Zufällige Position innerhalb der BoundingBox
            float x = Random.Range(center.x - half.x, center.x + half.x);
            float y = Random.Range(center.y - half.y, center.y + half.y);
            Vector2 spawnPos = new Vector2(x, y);

            GameObject go = Instantiate(fireflyPrefab, spawnPos, Quaternion.identity, transform);
            BeatFirefly f = go.GetComponent<BeatFirefly>();

            if (f != null)
                fireflies.Add(f);

            // Drift bekommt automatisch die BoundingBox als Manager-Transform
            //FireflyDrift drift = go.GetComponent<FireflyDrift>();
            FireflyDriftPhysics drift = go.GetComponent<FireflyDriftPhysics>();
            if (drift != null)
                drift.boundingBox = this.transform;
        }
    }

    private void HandleBeatSoundPlayed(BeatSound source)
    {
        // An alle Firefly Objekte weitergeben
        foreach (var f in fireflies)
            f.Flash();
    }
}