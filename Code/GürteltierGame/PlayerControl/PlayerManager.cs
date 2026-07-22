using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public int starCount;
    public int fruitCount;
    public bool sprintUnlocked;
    public bool doubleJumpUnlocked;
    public bool dashUnlocked;
    public bool gravitySwitchUnlocked;
    public bool swimUnlocked;

    [SerializeField] private TextMeshProUGUI starCountText;
    [SerializeField] private Image starImage;
    [SerializeField] private float animationScale = 1.4f;
    [SerializeField] private float animationDuration = 0.15f;
    [SerializeField] private TextMeshProUGUI fruitCountText;
    [SerializeField] private Image fruitImage;

    [SerializeField] private float fruitToUnlockSprint = 20;
    [SerializeField] private GameObject sprintPopup;
    [SerializeField] private float fruitToUnlockDoubleJump = 40;
    [SerializeField] private GameObject doubleJumpPopup;

    public void AddStar()
    {
        starCount++;
        starCountText.text = starCount.ToString() + " x ";
        StartCoroutine(PlayPopEffect(starImage));
    }
    public void AddFruit()
    {
        fruitCount++;
        fruitCountText.text = fruitCount.ToString() + " x ";
        StartCoroutine(PlayPopEffect(fruitImage));
        if (fruitCount == fruitToUnlockSprint)
        {
            GameManager.Instance.ShowPopup(sprintPopup);
            sprintUnlocked = true;
        }
        if (fruitCount == fruitToUnlockDoubleJump)
        {
            GameManager.Instance.ShowPopup(doubleJumpPopup);
            doubleJumpUnlocked = true;
        }
    }

    private IEnumerator PlayPopEffect(Image popImage)
    {
        Vector3 originalScale = popImage.rectTransform.localScale;
        Vector3 targetScale = originalScale * animationScale;

        // Scale up
        float t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            popImage.rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, t / animationDuration);
            yield return null;
        }

        // Scale back
        t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            popImage.rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, t / animationDuration);
            yield return null;
        }

        popImage.rectTransform.localScale = Vector3.one; // just to be sure
    }
}