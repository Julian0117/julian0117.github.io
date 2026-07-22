using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] public float walkSpeed = 8f;
    public float sprintMultiplier = 1.5f;
    private bool isSprinting;

    [Header("Jump Settings")]
    //private bool canDoubleJump = false;
    [SerializeField]
    private float jumpStrength = 25f;
    [SerializeField] private float jumpBeatMultiplier = 1.5f;
    [SerializeField] LayerMask groundLayer;
    private CapsuleCollider2D groundCheckCollider;

    // Player Body used for movement and control
    private Rigidbody2D rb;
    private Vector2 moveInput;

    /* --- World Interaction --- */
    public Interactable currentInteractable; // Currently active Interactable, detected if its collider is overlapping with the player's

    // Pulling objects towards player and holding them
    private PickupItem heldObject; // Object currently held by the player
    [Header("Pickup Settings")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pullForce = 20f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float dampingFactor = 4f;

    [Header("Throw Settings")]
    // Throwing held objects
    private ThrowTrajectory trajectory;
    [SerializeField] private float throwStrength = 5f;
    private float heldObjectGravity;

    /* --- Animation Controller --- */
    private Animator playerAnimator;


    private void Awake()
    {
        groundCheckCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        //playerSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (heldObject != null)
        {
            trajectory.Show(throwStrength, heldObjectGravity, CalculateThrowStrength());
        }
    }

    private void FixedUpdate()
    {
        PlayerMovement();
        
        if (heldObject != null) LevitateObject();
    }

    // Sets the 2D vector moveInput to the current keyboard / controller input
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        //Debug.Log($"Move Input: {moveInput}");
    }

    // Is called every frame by FixedUpdate() and applies the current
    // walkSpeed to the direction set by the OnMove method
    private void PlayerMovement()
    {
        float speed = walkSpeed * (isSprinting ? sprintMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

        if (moveInput.x != 0)
        {
            if (moveInput.x > 0)
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);        // Blick rechts
            else
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);      // Blick links

            playerAnimator.SetBool("Walking", true);
        }
        else
        {
            playerAnimator.SetBool("Walking", false);
        }
    }


    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded())
        {
            //bool onBeat = BeatManager.Instance.IsOnBeat();
            bool onBeat = false;

            float force = onBeat ? jumpStrength * jumpBeatMultiplier : jumpStrength;

            if (onBeat)
                Debug.Log("Perfect timing!");

            playerAnimator.SetTrigger("Jump");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            //rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
    }

    // Checks whether the player is currently touching the ground layer
    // allowing them to jump
    public bool IsGrounded()
    {
        return groundCheckCollider.IsTouchingLayers(groundLayer);
    }

    public void AddInteractable(Interactable i)
    {
        Debug.Log("AddInteractable called for " + i.name);
        if(currentInteractable != null) currentInteractable.HideOutline();
        currentInteractable = i;
        currentInteractable.ShowOutline();
    }
    
    public void RemoveInteractable()
    {
        //Debug.Log("RemoveInteractable called");
        currentInteractable.HideOutline();
        currentInteractable = null;
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (heldObject != null)
        {
            var interactable = heldObject.GetComponent<Interactable>();
            interactable?.Interact(this);
            trajectory.Hide();
        }
        else if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }
    
    public void Pickup(PickupItem i)
    {
        heldObject = i;
    
        Rigidbody2D rb = i.GetComponent<Rigidbody2D>();
        Collider2D c = i.GetComponent<PolygonCollider2D>();
        
        if (c != null) c.isTrigger = false;
        
        heldObjectGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.bodyType = RigidbodyType2D.Dynamic;

        trajectory = i.GetComponent<ThrowTrajectory>();

        DisableCollision();
    }

    private void LevitateObject()
    {
        Rigidbody2D rb = heldObject.GetComponent<Rigidbody2D>();

        Vector2 targetPos = holdPoint.position;
        Vector2 currentPos = rb.position;

        Vector2 toTarget = targetPos - currentPos;

        // PROPORTIONALER ANTEIL
        Vector2 pForce = toTarget * pullForce; // wirkt Richtung Ziel

        // DIFFERENZIELLER ANTEIL (bremst Oszillation)
        Vector2 dForce = -rb.linearVelocity * dampingFactor;

        // Gesamt
        Vector2 force = pForce + dForce;

        rb.AddForce(force);

        // Geschwindigkeitsgrenze
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }
    public void Drop()
    {
        if (heldObject == null) return;

        Rigidbody2D rb = heldObject.GetComponent<Rigidbody2D>();
        rb.gravityScale = heldObjectGravity;

        EnableCollision(heldObject);
        heldObject = null;
    }

    // Method to throw held objects
    public void OnThrow(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || heldObject == null) return;

        trajectory.Hide();

        Rigidbody2D rb = heldObject.GetComponent<Rigidbody2D>();
        rb.gravityScale = heldObjectGravity;


        Vector2 throwVelocity = CalculateThrowStrength();
        rb.linearVelocity = throwVelocity;

        heldObject.OnThrown();
        Debug.Log($"{heldObject} wurde geworfen mit Geschwindigkeit {throwVelocity}!");

        EnableCollision(heldObject);
        heldObject = null;
    }

    private Vector2 CalculateThrowStrength()
    {
        Vector2 objectPos = heldObject.transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Vector2 toMouse = mousePos - objectPos;

        // Normale Richtung
        Vector2 dir = toMouse.normalized;

        // Distanz berechnen
        float dist = toMouse.magnitude;

        // Clamping
        float minDist = 0f;    // unter diesem Wert wird der Wurf nicht komplett winzig
        float maxDist = 10f;      // maximale Wurfstärke-Entfernung

        float scaledDist = Mathf.Clamp(dist, minDist, maxDist);

        // tatsächliche Geschwindigkeit zurückgeben
        return dir/* * scaledDist*/ * throwStrength;
    }

    // Disables collision between player and held object
    private void DisableCollision()
    {
        var playerCol = GetComponent<Collider2D>();
        var itemCol = heldObject.GetComponent<Collider2D>();

        Physics2D.IgnoreCollision(playerCol, itemCol, true);
    }

    // Enables collision between player and held object and reset its linearDamping value after a small delay
    public void EnableCollision(PickupItem item)
    {
        StartCoroutine(EnableCollisionRoutine(item, 0.1f));
    }

    private IEnumerator EnableCollisionRoutine(PickupItem item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (item == null) yield break;

        var playerCol = GetComponent<Collider2D>();
        var itemCol = item.GetComponent<Collider2D>();

        Physics2D.IgnoreCollision(playerCol, itemCol, false);
    }
}