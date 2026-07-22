using UnityEngine;

public class WaterDrop : BeatSound
{
    [SerializeField] private Droplet dropletPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int poolSize = 10;

    private Droplet[] pool;
    private int index = 0;

    protected override void Awake()
    {
        base.Awake();

        pool = new Droplet[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            Droplet d = Instantiate(dropletPrefab, spawnPoint.position, Quaternion.identity);
            d.Init(spawnPoint, OnDropletPlayed);
            d.transform.SetParent(spawnPoint);
            d.audioClip = audioClip;
            pool[i] = d;
        }
    }

    protected override void Play()
    {
        AudioClip clip = audioClip;

        Droplet d = pool[index];
        index = (index + 1) % poolSize;

        d.Spawn();
    }

    private void OnDropletPlayed(Droplet d)
    {
        beatManager.NotifyBeatSoundPlayed(this);
    }
}