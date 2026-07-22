using UnityEngine;
using System.Collections;

public class OwlSwitch : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;
    public AudioSource audioSource;
    public AudioClip owlSound;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = closedSprite;   // 开始闭眼
    }

    public void Activate()
    {
        sr.sprite = openSprite;
        StartCoroutine(CallOwl());
    }

    IEnumerator CallOwl()
    {
        while (true)     // 无限循环
        {
            audioSource.PlayOneShot(owlSound);
            yield return new WaitForSeconds(2f);   // 每2秒叫一次
        }
    }
}
