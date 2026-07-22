using UnityEngine;

public class WoodpeckerTrigger : MonoBehaviour
{
    [SerializeField] private BeatSound beatSound;
    private SpriteRenderer larvaRenderer;

    private void Awake()
    {
        larvaRenderer = GetComponentInChildren<SpriteRenderer>();
        larvaRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Larva"))
        {
            beatSound.FirstEnable();
            larvaRenderer.enabled = true;
            other.gameObject.SetActive(false);
        }
    }
}