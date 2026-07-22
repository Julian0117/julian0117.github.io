using UnityEngine;

public class RabbitTrigger : PickupItem
{
    private Rigidbody2D carrot;
    [SerializeField] private BeatSound beatSound;
    [SerializeField] private GameObject rabbitPaw;

    void Start()
    {
        carrot = GetComponent<Rigidbody2D>();
    }

    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        carrot.bodyType = RigidbodyType2D.Dynamic;
        beatSound.FirstEnable();
        rabbitPaw.SetActive(false);
        //Debug.Log($"Passed {this} FirstEnable()");
    }
}