using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FlowerGrowth : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] growthStages;

    [Header("Timing")]
    [SerializeField] private float timeBetweenStages = 0.25f;

    [Header("Stage Limits")]
    [SerializeField] private int stage1EndIndex = 10;
    [SerializeField] private int stage2EndIndex = 14;

    [Header("Water")]
    [SerializeField] private int waterThreshold = 4;

    [Header("BeeTrigger")]
    [SerializeField] private BeeTrigger beeTrigger;

    private int waterCounter = 0;
    private int currentStage = 0;
    private bool isGrowing = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (growthStages.Length > 0)
            spriteRenderer.sprite = growthStages[0];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Water"))
            return;

        if (isGrowing)
            return;

        waterCounter++;

        if (waterCounter >= waterThreshold)
        {
            StartCoroutine(GrowPlant());
        }
    }

    private IEnumerator GrowPlant()
    {
        yield return StartCoroutine(AdvanceGrowth(stage1EndIndex));

        if (beeTrigger != null)
        {
            beeTrigger.flowerGrown = true;
            beeTrigger.BeeCheck();
        }
    }

    public void BloomPlant()
    {
        if (isGrowing)
            return;

        StartCoroutine(AdvanceGrowth(stage2EndIndex));
    }

    private IEnumerator AdvanceGrowth(int targetIndex)
    {
        isGrowing = true;

        targetIndex = Mathf.Clamp(targetIndex, 0, growthStages.Length - 1);

        while (currentStage < targetIndex)
        {
            yield return new WaitForSeconds(timeBetweenStages);
            currentStage++;
            spriteRenderer.sprite = growthStages[currentStage];
        }

        isGrowing = false;
    }
}