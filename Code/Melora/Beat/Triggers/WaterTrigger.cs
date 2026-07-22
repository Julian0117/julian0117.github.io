using UnityEngine;

public class WaterTrigger : PickupItem
{
    private Rigidbody2D rock;
    [SerializeField] private BeatSound beatSound;

    void Start()
    {
        rock = GetComponent<Rigidbody2D>();
    }

    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        rock.bodyType = RigidbodyType2D.Dynamic;
        beatSound.FirstEnable();
    }
}