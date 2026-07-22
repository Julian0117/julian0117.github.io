using UnityEngine;
using UnityEngine.EventSystems;


// Abstrakte Klasse Interactable die von verschiedenen interaktiven Objekten geerbt werden kann
//[RequireComponent(typeof(InteractableOutline))]
public abstract class Interactable : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    protected InteractableOutline outline;
    //[SerializeField] PlayerController player;

    protected virtual void Awake()
    {
        //Debug.Log($"Interactable Awake() {this} called");
        //player = GameObject.Find("Player").GetComponent<PlayerController>();
        outline = GetComponentInChildren<InteractableOutline>();
        if (outline == null) Debug.Log("No InteractableOutline found");
    }

    public abstract void Interact(PlayerController player);

    /*public void OnPointerEnter(PointerEventData eventData)
    {
        //player.AddInteractable(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //player.RemoveInteractable();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }*/
    
    public void ShowOutline()
    {
        outline.ShowOutline();
    }

    public void HideOutline()
    {
        outline.HideOutline();
    }
}