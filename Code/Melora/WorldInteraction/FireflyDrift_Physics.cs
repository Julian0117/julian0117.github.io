using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireflyDriftPhysics : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float driftForce = 0.5f;
    [SerializeField] private float noiseScale = 0.5f;
    [SerializeField] private float directionChangeSpeed = 2f;

    [Header("Bounding Box")]
    public Transform boundingBox;
    [SerializeField] private float boundaryInfluence = 3f;

    private Rigidbody2D rb;

    private Vector2 currentDirection;
    private float noiseOffsetX;
    private float noiseOffsetY;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        noiseOffsetX = Random.value * 999f;
        noiseOffsetY = Random.value * 999f;

        currentDirection = Random.insideUnitCircle.normalized;
    }

    void FixedUpdate()
    {
        float t = Time.time;

        // Noise Direction
        float nx = Mathf.PerlinNoise(t * noiseScale, noiseOffsetX) * 2f - 1f;
        float ny = Mathf.PerlinNoise(t * noiseScale, noiseOffsetY) * 2f - 1f;
        Vector2 noiseDir = new Vector2(nx, ny).normalized;

        // Bounding Box
        Vector2 center = boundingBox.position;
        Vector2 half = boundingBox.localScale * 0.5f;
        Vector2 pos = rb.position;

        Vector2 boundaryDir = Vector2.zero;

        if (pos.x < center.x - half.x) boundaryDir.x = 1f;
        else if (pos.x > center.x + half.x) boundaryDir.x = -1f;

        if (pos.y < center.y - half.y) boundaryDir.y = 1f;
        else if (pos.y > center.y + half.y) boundaryDir.y = -1f;

        Vector2 targetDir = noiseDir;

        if (boundaryDir != Vector2.zero)
            targetDir = (noiseDir + boundaryDir.normalized * boundaryInfluence).normalized;

        currentDirection = Vector2.Lerp(
            currentDirection,
            targetDir,
            directionChangeSpeed * Time.fixedDeltaTime
        ).normalized;

        // Kraft anwenden → Spieler kann Firefly wegschieben
        rb.AddForce(currentDirection * driftForce, ForceMode2D.Force);
    }
}