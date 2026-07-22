using UnityEngine;

public class PushObjects : MonoBehaviour
{
    [SerializeField] private float pushForce = 6f;

    private GravityBody gravityBody;

    private void Awake()
    {
        gravityBody = GetComponent<GravityBody>();
    }

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody other = collision.rigidbody;

        if (other != null && !other.isKinematic && other != GetComponent<Rigidbody>())
        {
            // Prüfe, ob einer der Kontaktpunkte annähernd senkrecht zur Gravitation ist
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 gravityDir = gravityBody.GravityDirection.normalized;
                float dot = Mathf.Abs(Vector3.Dot(contact.normal, gravityDir));

                if (dot < 0.3f) // Oberfläche ist fast senkrecht zur Gravitation = Wand
                {
                    Vector3 pushDir = (other.transform.position - transform.position).normalized;
                    Vector3 targetVelocity = pushDir * pushForce;
                    other.linearVelocity = Vector3.Lerp(other.linearVelocity, targetVelocity, 0.1f);
                    break; // Nur einmal pro Kollisionsereignis pushen
                }
            }
        }
    }
}