using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteract : MonoBehaviour
{
    private Door door;
    private InteractableOutline outline;
    private Transform player;

    private bool inside = false;

    private void Start()
    {
        // Door am Parent finden
        door = GetComponentInParent<Door>();
        if (door == null)
        {
            Debug.LogError("[DoorInteract] Keine Door im Parent gefunden!");
            return;
        }

        // Outline am VISUAL-Child der Tür finden
        if (door.visual != null)
            outline = door.visual.GetComponent<InteractableOutline>();

        if (outline == null)
            Debug.LogError("[DoorInteract] Keine InteractableOutline am door.visual gefunden!", this);
        else
            outline.HideOutline(); // Start = Outline aus
    }

    private void Update()
    {
        if (!inside)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (player != null)
            {
                // Spielerposition übergeben für Öffnungsrichtung
                door.Toggle(player.position);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        inside = true;
        player = other.transform;

        if (outline != null)
            outline.ShowOutline();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        inside = false;
        player = null;

        if (outline != null)
            outline.HideOutline();
    }
}
