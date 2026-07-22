using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class ThrowTrajectory : MonoBehaviour
{
    [Header("Trajectory")]
    [SerializeField] private int segmentCount = 200;
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Show(float throwStrength, float heldObjectGravity, Vector2 throwVelocity)
    {
        lineRenderer.positionCount = segmentCount;
        
        Vector2 startPos = transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Vector2 gravity = Physics2D.gravity * heldObjectGravity;
        float dt = Time.fixedDeltaTime;

        Vector2 pos = startPos;

        for (int i = 0; i < segmentCount; i++)
        {
            lineRenderer.SetPosition(i, pos);

            throwVelocity += gravity * dt;
            pos += throwVelocity * dt;
        }
    }

    public void Hide()
    {
        lineRenderer.positionCount = 0;
    }
}