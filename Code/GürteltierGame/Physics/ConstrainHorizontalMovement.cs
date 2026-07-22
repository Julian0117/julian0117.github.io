using UnityEngine;

public class ConstrainHorizontalMovement : MonoBehaviour
{
    private Vector3 initialLocalPos;

    void Start()
    {
        // Lokale Position am Start speichern
        initialLocalPos = transform.localPosition;
    }

    void FixedUpdate()
    {
        // Nur X und Z zurücksetzen, Y bleibt wie er ist
        Vector3 currentLocalPos = transform.localPosition;
        transform.localPosition = new Vector3(
            initialLocalPos.x,
            currentLocalPos.y,
            initialLocalPos.z
        );
    }
}
