using UnityEngine;

public class PowerupParticles : MonoBehaviour
{
    private ParticleSystem dissolveParticles;

    private void Awake()
    {
        // Erstes Partikelsystem im gesamten Kind-Baum finden
        dissolveParticles = GetComponentInChildren<ParticleSystem>(true);

        if (dissolveParticles == null)
            Debug.LogWarning("ParticleSystem nicht gefunden!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Partikel starten
            if (dissolveParticles != null)
                dissolveParticles.transform.SetParent(null); // Detach
                dissolveParticles.Play();
        }
    }
}