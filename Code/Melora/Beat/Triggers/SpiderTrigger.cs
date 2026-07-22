using UnityEngine;

public class SpiderTrigger : PickupItem
{
    private Rigidbody2D fly;
    [SerializeField] private BeatSound beatSound;

    void Start()
    {
        fly = GetComponent<Rigidbody2D>();
    }

    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        fly.bodyType = RigidbodyType2D.Dynamic;
        beatSound.FirstEnable();
    }
}