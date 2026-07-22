using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Tooltip("Startposition des Spielers beim Respawn.")]
    public Vector3 respawnPosition;

    [Tooltip("Tag der Objekte, bei deren Kollision der Spieler zurückgesetzt wird.")]
    public string respawnTag = "Respawn";

    [Tooltip("Verlangsamung durch Wasser")]
    public float waterSlowdownNoUpgrade = 0.1f;
    public float waterSlowdownUpgrade = 0.5f;

    [Tooltip("Zeit bis zum Respawn")]
    public float respawnTime = 3.0f;

    private GameObject player;
    [SerializeField] private GameObject snorkel;

    private PlayerInputController movementScript;
    private PlayerManager playerManager;
    private float originalWalkSpeed;
    private float originalSprintMultiplier;

    private Coroutine respawnCoroutine;

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        if (respawnPosition == Vector3.zero)
        {
            respawnPosition = player.transform.position;
        }

        movementScript = player.GetComponent<PlayerInputController>();
        if (movementScript != null)
        {
            originalWalkSpeed = movementScript.walkSpeed;
            originalSprintMultiplier = movementScript.sprintMultiplier;
        } else
        {
            Debug.Log("Failed to get Movement Component");
        }
        playerManager = player.GetComponent<PlayerManager>();
    }

    public void RespawnCheck()
    {
        {
            if (!playerManager.swimUnlocked && respawnCoroutine == null)
            {
                Debug.Log("Respawn in " + respawnTime + "...");
                // Bewegungsgeschwindigkeit auf 10 % reduzieren
                if (movementScript != null)
                {
                    movementScript.walkSpeed = originalWalkSpeed * waterSlowdownNoUpgrade;
                    movementScript.sprintMultiplier = 1f;
                }
                respawnCoroutine = StartCoroutine(DelayedRespawn());
            } 
            else if (playerManager.swimUnlocked)
            {
                // Bewegungsgeschwindigkeit auf 50 % reduzieren
                if (movementScript != null)
                {
                    movementScript.walkSpeed = originalWalkSpeed * waterSlowdownUpgrade;
                    movementScript.sprintMultiplier = 1f;
                }
                snorkel.SetActive(true);
            }
        }
    }

    private void Respawn()
    {
        player.transform.position = respawnPosition;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void AbortRespawn ()
    {
        snorkel.SetActive(false);
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;

            Debug.Log("Respawn aborted!");
        }

        // Bewegungsgeschwindigkeit zurücksetzen
        if (movementScript != null)
        {
            movementScript.walkSpeed = originalWalkSpeed;
            movementScript.sprintMultiplier = originalSprintMultiplier;
        }
    }

    private IEnumerator DelayedRespawn()
    {
        

        yield return new WaitForSeconds(respawnTime);

        Respawn();

        // Geschwindigkeit wiederherstellen
        if (movementScript != null)
        {
            movementScript.walkSpeed = originalWalkSpeed;
            movementScript.sprintMultiplier = originalSprintMultiplier;
        }

        respawnCoroutine = null;
    }
}