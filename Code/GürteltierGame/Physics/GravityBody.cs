using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[System.Serializable]
public class GravityBody : MonoBehaviour
{
    //public float GRAVITY_FORCE = 1200; // Jetzt in GravityArea enthalten
    public float _activeGravityForce;

    private Rigidbody _rigidbody;
    private List<GravityArea> _gravityAreas = new();
    public GravityArea _activeGravityArea;

    public Vector3 GravityDirection => _activeGravityArea != null
        ? _activeGravityArea.GetGravityDirection(this).normalized
        : Vector3.zero;

    [SerializeField] private float slopeThreshold = 25f; // Grad
    [SerializeField] private bool debug = false;
    [SerializeField] private float groundCheckDistance = 0.5f;

    private int excludeMask;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        int waterLayer = LayerMask.NameToLayer("Water");
        int waterSensorLayer = LayerMask.NameToLayer("WaterSensor");

        // Maske für beide auszuschließenden Layer erstellen
        excludeMask = ~((1 << waterLayer) | (1 << waterSensorLayer));
    }

    void FixedUpdate()
    {
        if (debug)
        {
            UpdateActiveGravityArea();

            if (_activeGravityArea != null)
            {
                Vector3 gravityForce = GravityDirection * (_activeGravityForce * Time.fixedDeltaTime);
                _rigidbody.AddForce(gravityForce, ForceMode.Acceleration);

                

                // Stabilisierung gegen Hangabrollen
                if (Physics.Raycast(transform.position, GravityDirection, out RaycastHit hit, groundCheckDistance, excludeMask))
                {
                    // Zeichne GroundCheck (grün)
                    Debug.DrawRay(transform.position, gravityForce.normalized * 5f, Color.green);

                    Vector3 surfaceNormal = hit.normal;
                    float slopeAngle = Vector3.Angle(surfaceNormal, -GravityDirection);

                    // Debug-Ausgabe des getroffenen Objekts
                    Debug.Log($"[GravityBody] Boden erkannt: {transform.name} trifft {hit.collider.name} bei {hit.distance:F2}m Entfernung.");

                    // Zeichne Normale (blau)
                    Debug.DrawRay(hit.point, surfaceNormal * 5f, Color.blue);

                    if (slopeAngle < slopeThreshold)
                    {
                        // Hangrichtung (also Richtung, in die die Kugel rollt)
                        Vector3 slopeDirection = Vector3.ProjectOnPlane(GravityDirection, surfaceNormal).normalized;

                        // Kraft berechnen, die hangabwärts wirkt:
                        float gravityAlongSlope = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * _activeGravityForce;

                        // Gegenkraft exakt entgegen der Hangrichtung
                        Vector3 counterForce = -slopeDirection * gravityAlongSlope * Time.fixedDeltaTime;

                        _rigidbody.AddForce(counterForce, ForceMode.Acceleration);

                        if (debug)
                        {
                            Debug.DrawRay(transform.position, counterForce.normalized * 2f, Color.red);
                            Debug.Log($"[GravityBody] Gegenkraft auf Hang ({slopeAngle:F2}°): {counterForce.magnitude:F3}");
                        }
                    }
                }
            }
        }
        else
        {
            UpdateActiveGravityArea();

            if (_activeGravityArea != null)
            {
                Vector3 gravityForce = GravityDirection * (_activeGravityForce * Time.fixedDeltaTime);
                _rigidbody.AddForce(gravityForce, ForceMode.Acceleration);

                // Stabilisierung gegen Hangabrollen
                if (Physics.Raycast(transform.position, GravityDirection, out RaycastHit hit, groundCheckDistance, excludeMask))
                {
                    Vector3 surfaceNormal = hit.normal;
                    float slopeAngle = Vector3.Angle(surfaceNormal, -GravityDirection);

                    if (slopeAngle < slopeThreshold)
                    {
                        // Hangrichtung (also Richtung, in die die Kugel rollt)
                        Vector3 slopeDirection = Vector3.ProjectOnPlane(GravityDirection, surfaceNormal).normalized;

                        // Kraft berechnen, die hangabwärts wirkt:
                        float gravityAlongSlope = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * _activeGravityForce;

                        // Gegenkraft exakt entgegen der Hangrichtung
                        Vector3 counterForce = -slopeDirection * gravityAlongSlope * Time.fixedDeltaTime;

                        _rigidbody.AddForce(counterForce, ForceMode.Acceleration);
                    }
                }
            }
        }
    }

    public void AddGravityArea(GravityArea gravityArea)
    {
        _gravityAreas.Add(gravityArea);
        UpdateActiveGravityArea();
    }

    public void RemoveGravityArea(GravityArea gravityArea)
    {
        _gravityAreas.Remove(gravityArea);
        UpdateActiveGravityArea();
    }

    private void UpdateActiveGravityArea()
    {
        // Entferne ungültige oder deaktivierte GravityAreas
        _gravityAreas = _gravityAreas
            .Where(a => a != null && a.enabled && a.gameObject.activeInHierarchy)
            .ToList();

        if (_gravityAreas.Count == 0)
        {
            _activeGravityArea = null;
            return;
        }

        _gravityAreas.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        var newActiveArea = _gravityAreas.Last();

        if (newActiveArea != _activeGravityArea)
        {
            _activeGravityArea = newActiveArea;
            _activeGravityForce = _activeGravityArea.Gravity;
            _activeGravityArea?.OnBecameActive(this);
        }
    }

    public GravityArea GetActiveGravityArea() => _activeGravityArea;
}