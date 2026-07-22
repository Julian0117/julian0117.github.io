using UnityEngine;

public class BeeTrigger : MonoBehaviour
{
    [SerializeField] private BeatSound beatSound;
    private SpriteRenderer beeSprite;
    public bool flowerGrown = false;
    public bool sunShining = false;

    private void Start()
    {
        beeSprite =  beatSound.GetComponentInChildren<SpriteRenderer>();
        beeSprite.enabled = false;
    }

    public void BeeCheck()
    {
        beeSprite.enabled = true;
        if (flowerGrown && sunShining) beatSound.FirstEnable();
    }
}