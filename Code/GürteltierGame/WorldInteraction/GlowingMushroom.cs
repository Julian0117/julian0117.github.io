using UnityEngine;

public class GlowingMushroom : MonoBehaviour
{
    [SerializeField] private float bounceForce = 8f;
    [SerializeField] private bool useGravityDirection = false;
    private bool isGlowing = false;

    private Renderer rend;
    private Material matInstance;
    private Light pointLight;
    private Light spotLight;

    private Color originalEmissionColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        matInstance = new Material(rend.material);
        rend.material = matInstance;

        originalEmissionColor = matInstance.GetColor("_EmissionColor");

        matInstance.SetColor("_EmissionColor", Color.black);

        Light[] childLights = GetComponentsInChildren<Light>(true);
        foreach (var light in childLights)
        {
            if (light.type == LightType.Point && pointLight == null)
            {
                pointLight = light;
            }
            else if (light.type == LightType.Spot && spotLight == null)
            {
                spotLight = light;
            }
        }

        if (pointLight == null)
        {
            Debug.LogWarning("Kein PointLight als Child gefunden.");
        }

        if (spotLight == null)
        {
            Debug.LogWarning("Kein SpotLight als Child gefunden.");
        }

        // Initialstatus: nur SpotLight aktiv
        if (spotLight) spotLight.enabled = true;
        if (pointLight) pointLight.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ToggleGlow();

        Rigidbody rb = collision.rigidbody;

        if (rb != null && !rb.isKinematic)
        {
            Vector3 bounceDir = transform.up;

            if (useGravityDirection && rb.TryGetComponent(out GravityBody gravityBody))
            {
                bounceDir = (gravityBody.transform.position - transform.position).normalized;
            }

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);
        }
    }

    public void ToggleGlow()
    {
        if (!isGlowing)
        {
            matInstance.SetColor("_EmissionColor", originalEmissionColor);

            if (pointLight) pointLight.enabled = true;
            if (spotLight) spotLight.enabled = false;

            isGlowing = true;
        }
        else
        {
            matInstance.SetColor("_EmissionColor", Color.black);

            if (pointLight) pointLight.enabled = false;
            if (spotLight) spotLight.enabled = true;

            isGlowing = false;
        }
    }
}