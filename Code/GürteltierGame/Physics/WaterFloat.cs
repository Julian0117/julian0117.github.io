using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WaterFloat : MonoBehaviour
{
    [SerializeField] private float floatStrength = 20f;
    [SerializeField] private float bobbingAmplitude = 0.5f;
    [SerializeField] private float bobbingFrequency = 1.5f;
    [SerializeField] private float alignTorqueStrength = 1;
    [SerializeField] private float buoyancyFactor = 1.5f;

    private readonly Dictionary<GravityBody, List<Transform>> buoyantBodies = new();

    private SphereCollider waterCollider;
    private PlayerController playerController;

    private void Awake()
    {
        waterCollider = GetComponent<SphereCollider>();
        playerController = FindAnyObjectByType<PlayerController>();
        waterCollider.isTrigger = true;
    }

    private void FixedUpdate()
    {
        foreach (var kvp in buoyantBodies)
        {
            GravityBody body = kvp.Key;
            List<Transform> buoys = kvp.Value;
            ApplyBuoyancy(body, buoys);
        }
    }

    private void ApplyBuoyancy(GravityBody gravityBody, List<Transform> buoyPoints)
    {
        if (gravityBody == null || buoyPoints == null || buoyPoints.Count == 0) return;

        Rigidbody rb = gravityBody.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 waterCenter = transform.position;
        float waterRadius = waterCollider.radius * Mathf.Max(
            transform.lossyScale.x,
            transform.lossyScale.y,
            transform.lossyScale.z);

        Vector3 gravityDir = gravityBody.GravityDirection.normalized;
        Vector3 up = -gravityDir;

        bool buoyInWater = false;

        foreach (var buoy in buoyPoints)
        {
            Vector3 point = buoy.position;
            Vector3 toPoint = point - waterCenter;

            // Tiefe entlang Gravitationsrichtung
            float distanceFromSurface = -(Vector3.Dot(toPoint, gravityDir) + waterRadius);
            //Debug.Log($"Current Distance from Surface: '{distanceFromSurface}'");
            if (distanceFromSurface > 0f)
            {
                //Debug.Log("Punkt über Wasser");
                continue;
            }


            float depth = -distanceFromSurface;
            //Debug.Log($"Current depth: '{depth}'");
            if (depth > 0.15f) buoyInWater = true;
            float depthLift = Mathf.Pow(depth + 1f, buoyancyFactor);
            //Debug.Log("Current depthLift: " + depthLift);
            float time = Time.time + buoy.GetInstanceID();
            float bobbing = Mathf.Sin(time * bobbingFrequency) * bobbingAmplitude;

            Vector3 buoyancyForce = up * ((floatStrength * depthLift) + bobbing);
            //Debug.Log("Current buoyancyForce: " + buoyancyForce);

            rb.AddForceAtPosition(buoyancyForce, point, ForceMode.Acceleration);
        }
        if (!rb.CompareTag("Player") || buoyInWater)
        {
            ApplyUprightTorque(rb, up, rb.CompareTag("Player") && buoyInWater);
        }

        
    }

    private void ApplyUprightTorque(Rigidbody rb, Vector3 up, bool alignForward)
    {
        Quaternion targetRotation;

        if (alignForward)
        {
            // Erstelle Zielrotation aus projectedForward und up
            targetRotation = Quaternion.LookRotation(PlayerInputController.forwardOnPlane, up);
        }
        else
        {
            // Nur Up-Ausrichtung
            Vector3 objectUp = rb.transform.up;
            targetRotation = Quaternion.FromToRotation(objectUp, up) * rb.rotation;
        }

        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(rb.rotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        if (Mathf.Abs(angle) > 0.01f)
        {
            Vector3 torque = axis.normalized * angle * alignTorqueStrength;
            rb.AddTorque(torque, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collider :{other.name} hat WaterSphere betreten");
        if (other.gameObject.CompareTag("PlayerMagnet")) return;
        if (other.gameObject.CompareTag("WaterSensor")) playerController.RespawnCheck();

        if (other.TryGetComponent(out GravityBody gravityBody) && !buoyantBodies.ContainsKey(gravityBody))
        {
            List<Transform> buoys = new();

            foreach (Transform child in gravityBody.GetComponentsInChildren<Transform>())
            {
                if (child.GetComponent<SphereCollider>() != null && child != gravityBody.transform && !child.CompareTag("PlayerMagnet"))
                {
                    buoys.Add(child);
                }
            }

            if (buoys.Count > 0)
            {
                buoyantBodies.Add(gravityBody, buoys);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Collider :{other.name} hat WaterSphere verlassen");

        if (other.gameObject.CompareTag("WaterSensor"))
        {
            playerController.AbortRespawn();
        }

        if (other.TryGetComponent(out GravityBody gravityBody))
        {
            buoyantBodies.Remove(gravityBody);
        }
    }
}