using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(ThrowTrajectory))]
public class PickupItem : Interactable
{
    protected bool isHeld = false;
    public override void Interact(PlayerController player)
    {
        PickupItem thisInteractable = GetComponent<PickupItem>();
        if (!isHeld)
        {
            isHeld = true;
            player.Pickup(thisInteractable);
            Debug.Log($"Item {thisInteractable} aufgenommen!");
        }
        else if (isHeld)
        {
            isHeld = false;
            player.Drop();
            Debug.Log($"Item {thisInteractable} fallen gelassen!");
        }
    }

    public void OnThrown()
    {
        isHeld = false;
    }
}