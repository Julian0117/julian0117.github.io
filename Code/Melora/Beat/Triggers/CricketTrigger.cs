using UnityEngine;

public class CricketTrigger : MonoBehaviour
{
    [SerializeField] private BeatSound beatSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Player")) {
            beatSound.FirstEnable();
        }
    }
}