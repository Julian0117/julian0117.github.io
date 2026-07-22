using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MagnetController : MonoBehaviour
{
    public PlayerManager playerManager; // Set in Inspector or via script
    public float forceStrength = 10f;
    public float targetDistance = 2f; // Für Pull-Zustand

    private bool magnetPull = true; // false -> Push

    private SphereCollider magnetCollider;
    private List<GravityBody> disabledGravityBodies = new List<GravityBody>();

    [SerializeField] private ParticleSystem magnetPullFX;
    [SerializeField] private ParticleSystem magnetPushFX;
    private ParticleSystem activeMagnetFX;

    private void Awake()
    {
        playerManager = GetComponentInParent<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager wurde im Parent nicht gefunden.");
        }

        // Annahme: SphereCollider liegt auf dem ersten Child
        magnetCollider = GetComponent<SphereCollider>();
        if (magnetCollider == null || !magnetCollider.isTrigger)
        {
            Debug.LogError("MagnetController benötigt einen Trigger-SphereCollider als Kindobjekt.");
        }
    }

    private void OnEnable()
    {
        AudioController.Instance.PlayMagnetToggleSound(true);
        activeMagnetFX = magnetPull ? magnetPullFX : magnetPushFX;

        if (activeMagnetFX != null)
            activeMagnetFX.Play();
    }

    private void OnDisable()
    {
        AudioController.Instance.PlayMagnetToggleSound(false);
        if (activeMagnetFX != null)
            activeMagnetFX.Stop();
    }

    private void FixedUpdate()
    {
        ApplyMagnetForce();
    }

    private void ApplyMagnetForce()
    {
        Vector3 center = magnetCollider.transform.position;
        Collider[] colliders = Physics.OverlapSphere(center, magnetCollider.radius * magnetCollider.transform.lossyScale.x);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || rb.isKinematic || rb.gameObject == this.gameObject || rb.gameObject == playerManager.gameObject)
                continue;

            // GravityBody deaktivieren
            GravityBody gravityBody = rb.GetComponent<GravityBody>();
            if (gravityBody != null && gravityBody.enabled)
            {
                gravityBody.enabled = false;
                disabledGravityBodies.Add(gravityBody);
            }

            Vector3 direction = center - rb.position;

            if (magnetPull)
            {
                float distance = direction.magnitude;
                //Debug.Log($"Current Distance: {distance}");
                Debug.DrawLine(rb.position, magnetCollider.transform.position, Color.green); // Zeigt Abstand
                float delta = targetDistance - distance;
                Vector3 pull = direction.normalized * (-delta * forceStrength);
                //Debug.Log($"Current Pull: {pull}");
                rb.AddForce(pull, ForceMode.Force);
            }
            else if (!magnetPull)
            {
                rb.AddForce(-direction.normalized * forceStrength, ForceMode.Force);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GravityBody gb = other.GetComponent<GravityBody>();
        if (gb != null)
            gb.enabled = true;
    }

    public void SwitchMagnetDirection()
    {
        // Vorherigen FX stoppen
        if (activeMagnetFX != null && activeMagnetFX.isPlaying)
            activeMagnetFX.Stop();

        magnetPull = !magnetPull;

        // Neuen FX setzen und starten
        activeMagnetFX = magnetPull ? magnetPullFX : magnetPushFX;

        if (activeMagnetFX != null)
            activeMagnetFX.Play();
    }

    public void DisableAndClean()
    {
        foreach (GravityBody gb in disabledGravityBodies)
        {
            if (gb != null) gb.enabled = true;
        }
        disabledGravityBodies.Clear();

        gameObject.SetActive(false); // oder Collider deaktivieren, je nachdem
    }
}