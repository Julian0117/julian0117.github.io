using UnityEngine;

public class FireflyDrift : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float noiseScale = 0.5f;
    [SerializeField] private float directionChangeSpeed = 2f;

    [Header("Bounding Box")]
    [SerializeField] public Transform boundingBox;
    [SerializeField] private float boundaryInfluence = 3f;

    private Vector2 currentDirection;
    private Vector2 targetDirection;

    private float noiseOffsetX;
    private float noiseOffsetY;

    void Start()
    {
        noiseOffsetX = Random.value * 999f;
        noiseOffsetY = Random.value * 999f;

        currentDirection = Random.insideUnitCircle.normalized;
        targetDirection = currentDirection;
    }

    void Update()
    {
        float t = Time.time;

        // 1. Noise-basierte Zielrichtung
        float nx = Mathf.PerlinNoise(t * noiseScale, noiseOffsetX) * 2f - 1f;
        float ny = Mathf.PerlinNoise(t * noiseScale, noiseOffsetY) * 2f - 1f;
        Vector2 noiseDirection = new Vector2(nx, ny).normalized;

        // 2. Bounding Box aus Szene übernehmen
        Vector2 boxCenter = boundingBox.position;
        Vector2 boxSize = boundingBox.localScale;
        Vector2 half = boxSize * 0.5f;

        Vector2 pos = transform.position;

        float dx = 0f;
        float dy = 0f;

        // 3. Erkennen, wenn außerhalb
        if (pos.x < boxCenter.x - half.x) dx = 1f;
        else if (pos.x > boxCenter.x + half.x) dx = -1f;

        if (pos.y < boxCenter.y - half.y) dy = 1f;
        else if (pos.y > boxCenter.y + half.y) dy = -1f;

        // 4. Boundary-Richtung (zurück in die Box)
        Vector2 boundaryDir = new Vector2(dx, dy).normalized;

        // 5. Noise + Boundary kombinieren
        if (boundaryDir != Vector2.zero)
            targetDirection = (noiseDirection + boundaryDir * boundaryInfluence).normalized;
        else
            targetDirection = noiseDirection;

        // 6. Sanfter Übergang Richtung Zielrichtung
        currentDirection = Vector2.Lerp(
            currentDirection,
            targetDirection,
            directionChangeSpeed * Time.deltaTime
        ).normalized;

        // 7. Bewegen
        transform.position += (Vector3)(currentDirection * speed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        if (boundingBox == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boundingBox.position, boundingBox.localScale);
    }
}