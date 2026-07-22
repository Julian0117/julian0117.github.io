using System.Collections.Generic;
using UnityEngine;

public class FrogTrigger : MonoBehaviour
{
    [SerializeField] private BeatSound beatSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fly"))
        {
            beatSound.FirstEnable();
            other.gameObject.SetActive(false);
        }
    }
}