using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Trampoline : MonoBehaviour
{
    [SerializeField] private float bounceForce = 20f;
    [SerializeField] private bool useGravityDirection = true;

    Animator animator;
    AudioSource audioSource;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        audioSource.Play();

        Rigidbody rb = collision.rigidbody;

        if (rb != null && !rb.isKinematic)
        {
            Vector3 bounceDir = transform.up;

            if (useGravityDirection && rb.TryGetComponent(out GravityBody gravityBody))
            {
                bounceDir = (gravityBody.transform.position - transform.position).normalized;
            }

            if (animator != null)
            animator.SetTrigger("ObjectCollision");

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);
        }
    }
}
