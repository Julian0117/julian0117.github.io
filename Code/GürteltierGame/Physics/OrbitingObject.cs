using UnityEngine;

public class OrbitingObject : MonoBehaviour
{
    public Transform target;
    public float orbitDistance = 30f;
    public float orbitSpeed = 50f;

    private float angle = 0f;

    void Update()
    {
        if (target == null) return;

        // Winkel inkrementieren
        angle += orbitSpeed * Time.deltaTime;
        angle %= 360f;

        // Neue Position berechnen
        float radians = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * orbitDistance;
        transform.position = target.position + offset;

        // Optional: immer zur Mitte schauen
        //transform.LookAt(target);
    }
}