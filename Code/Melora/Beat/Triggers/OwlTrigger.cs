using UnityEngine;

public class OwlTrigger : MonoBehaviour
{
    [SerializeField] private BeatSound beatSound;
    private Rigidbody2D branch;

    private void Start()
    {
        branch = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            branch.bodyType = RigidbodyType2D.Dynamic;
            beatSound.FirstEnable();
        }
    }
}