using UnityEngine;

public class ParallaxLayers : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform subject;

    [Header("Parallax Factors")]
    [SerializeField, Range(-1f, 1f)] private float horizontalFactor = 0f;
    [SerializeField, Range(-1f, 1f)] private float verticalFactor = 0f;
    [SerializeField, Range(-5f, 5f)] private float verticalOffset = 0f;

    private Vector2 startPosition;
    private float startZ;

    //private Vector2 Travel => (Vector2)cam.transform.position - startPosition;
    [SerializeField] private Vector2 travel;
    [SerializeField] private Vector2 offset;

    private void Awake()
    {
        if (!cam)
            cam = Camera.main;

        if (!cam)
            Debug.LogError($"[{name}] No Camera assigned.");

        if (!subject)
            Debug.LogWarning($"[{name}] No subject assigned (optional).");

        startPosition = transform.position;
        startZ = transform.position.z;
    }

    private void Update()
    {
        //Vector2 travel = Travel;
        Vector2 travel = (Vector2)cam.transform.position - startPosition;

        /*Vector2 */
        offset = new Vector2(
            travel.x * horizontalFactor,
            travel.y * verticalFactor
        );

        Vector3 newPos = new Vector3(
            startPosition.x + offset.x,
            startPosition.y + offset.y + verticalOffset,
            startZ
        );

        transform.position = newPos;
    }
}