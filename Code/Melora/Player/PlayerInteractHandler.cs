using UnityEngine;

public class PlayerInteractHandler : MonoBehaviour
{
    [SerializeField] PlayerController player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Collider2D {collision.name} detected");
        Interactable i = collision.GetComponent<Interactable>();
        if (i != null)
        {
            Debug.Log($"Interactable {i.name} detected");
            BeatSound sound = i.GetComponent<BeatSound>();
            if (sound != null && !sound.isPlaying) return;
            player.AddInteractable(i);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable i = collision.GetComponent<Interactable>();
        if (i != null && i == player.currentInteractable)
        {
            player.RemoveInteractable();
        }
    }

}