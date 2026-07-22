using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GravityBody))]
public class PlayerInputController : MonoBehaviour
{
    private Vector3 gravityUp;
    private Vector3 camForward;
    private Vector3 camRight;
    //private Vector3 forwardOnPlane;
    public static Vector3 forwardOnPlane;
    private Vector3 rightOnPlane;

    [Header("Movement Settings")]
    public float walkSpeed = 16f;
    public float sprintMultiplier = 1.5f;
    public float jumpStrength = 12f;
    private bool isSprinting;

    [Header("Jump Settings")]
    public bool raycastHit = false;
    public bool isGrounded = false;
    private bool canDoubleJump = false;

    [Header("Dash Settings")]
    public bool canDash = true;
    private bool isChargingDash = false;
    private bool dashReady = false;
    private bool isDashing = false;

    private float chargeTimer = 0f;
    private float dashChargeTime = 0.75f;
    private Coroutine dashChargeCoroutine;
    public float dashForce = 20f;
    private float minChargeRotationSpeed = 45f;
    private float maxChargeRotationSpeed = 1840;

    //[Header("Magnet Settings")]
    private MagnetController magnetController;
    private bool magnetControllerActive = false;

    // Player Body
    private GravityBody gravityBody;
    private Rigidbody rb;

    private Vector2 moveInput;

    // Input Actions
    private PlayerInput playerInput;
    private InputAction moveAction;
    public InputAction jumpAction;
    public InputAction sprintAction;
    public InputAction dashAction;
    public InputAction interactAction;
    public InputAction switchGravityAction;
    public InputAction toggleMagnetAction;
    public InputAction toggleControlsAction;
    public InputAction pauseAction;

    // Interactable Object/NPC
    private Interactable currentInteractable;

    private PlayerManager playerManager;
    private AudioController audioController;
    private PlayerParticleController playerParticleController;

    // Ground Check
    [SerializeField] private GameObject groundCheck;
    [SerializeField] private float groundCheckDistance = 0.2f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerManager = GetComponent<PlayerManager>();
        playerParticleController = GetComponent<PlayerParticleController>();
        magnetController = GetComponentInChildren<MagnetController>(true);
        audioController = GameObject.Find("Audio Source").GetComponent<AudioController>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        dashAction = playerInput.actions["Dash"];
        interactAction = playerInput.actions["Interact"];
        switchGravityAction = playerInput.actions["SwitchGravity"];
        toggleMagnetAction = playerInput.actions["ToggleMagnet"];
        toggleControlsAction = playerInput.actions["ToggleControls"];
        pauseAction = playerInput.actions["Pause"];
    }

    private void Start()
    {
        gravityBody = GetComponent<GravityBody>();
        rb = GetComponent<Rigidbody>();

        jumpAction.performed += OnJump;
        interactAction.performed += OnInteract;
        sprintAction.started += ctx => isSprinting = true;
        sprintAction.canceled += ctx => isSprinting = false;

        dashAction.started += OnDashStarted;
        dashAction.canceled += OnDashCanceled;

        switchGravityAction.performed += OnSwitchGravity;

        toggleMagnetAction.performed += OnToggleMagnet;
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        gravityUp = -gravityBody.GravityDirection;
        // Bewegung relativ zur Kamera
        camForward = Camera.main.transform.forward;
        camRight = Camera.main.transform.right;

        forwardOnPlane = Vector3.ProjectOnPlane(camForward, gravityUp).normalized;
        rightOnPlane = Vector3.ProjectOnPlane(camRight, gravityUp).normalized;

        //Vector3 moveDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        Vector3 moveDir = (forwardOnPlane * moveInput.y + rightOnPlane * moveInput.x).normalized;

        float currentSpeed = (isSprinting && playerManager.sprintUnlocked) ? walkSpeed * sprintMultiplier : walkSpeed;
        rb.AddForce(moveDir * currentSpeed, ForceMode.Acceleration);

        //CheckGroundStatus();

        // ******** MAGNET LOGIK **********
        /*if (!isMagnetActive) return;

        foreach (Transform obj in attachedObjects)
        {
            Vector3 targetPos = transform.position; // beliebiger Offset
            obj.position = Vector3.Lerp(obj.position, targetPos, Time.deltaTime * 5f);
        }*/
        // ********************************
    }

    // CheckGroundStatus mit Debugging
    private void CheckGroundStatus()
    {
        Vector3 origin = transform.position;
        Vector3 direction = gravityBody.GravityDirection;

        int waterLayer = LayerMask.NameToLayer("Water");
        int waterSensorLayer = LayerMask.NameToLayer("WaterSensor");

        // Maske für beide auszuschließenden Layer erstellen
        int excludeMask = ~((1 << waterLayer) | (1 << waterSensorLayer));

        isGrounded = Physics.Raycast(origin, direction, groundCheckDistance, excludeMask);
        if (isGrounded) canDash = true;
    }


    private void OnDashStarted(InputAction.CallbackContext ctx)
    {

        if (playerManager.dashUnlocked && canDash && !isDashing && !isChargingDash)
        {
            dashChargeCoroutine = StartCoroutine(ChargeDash());
            canDash = false;
        }
    }

    private void OnDashCanceled(InputAction.CallbackContext ctx)
    {
        if (isChargingDash)
        {
            StopCoroutine(dashChargeCoroutine);
            rb.constraints = RigidbodyConstraints.None;
            isChargingDash = false;

            // Egal was passiert: Ready-VFX deaktivieren
            playerParticleController.StopDashParticlesLoop();

            if (dashReady && !isDashing)
            {
                dashReady = false;
                StartCoroutine(PerformDash());
            }
            // Dash wird nur aufgebraucht, wenn dieser erfolgreich ausgeführt wurde
            else
            {
                canDash = true;
            }
        }
    }

    private IEnumerator ChargeDash()
    {
        isChargingDash = true;
        dashReady = false;

        chargeTimer = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        playerParticleController.PlayDashChargingLoop();


        while (true)
        {

            chargeTimer += Time.deltaTime;
            if (!dashReady && chargeTimer >= dashChargeTime)
            {
                dashReady = true;

                // Charging-VFX stoppen -> Ready-VFX starten
                //playerParticleController.StopDashParticlesLoop();
                playerParticleController.PlayDashReadyLoop();
            }

            // Rotationslogik
            float t = Mathf.Clamp01(chargeTimer / dashChargeTime);
            float rotationSpeed = Mathf.Lerp(minChargeRotationSpeed, maxChargeRotationSpeed, t);

            Vector3 localXAxis = Vector3.Cross(gravityUp, forwardOnPlane).normalized;
            transform.Rotate(localXAxis, rotationSpeed * Time.deltaTime, Space.World);

            yield return null;
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;

        Vector3 dashDir = Vector3.ProjectOnPlane(camForward, gravityUp).normalized;

        // Partikeleffekt starten
        playerParticleController.PlayDashParticlesBurst(forwardOnPlane);
        // Sound abspielen
        AudioController.Instance.PlayDashSound();

        GetComponent<DWGDestroyer>().enabled = true; // Aktiviert DWGDestroyer um mit zerstörbaren Objekten interagieren zu können

        // Starker Initialimpuls
        rb.AddForce(dashDir * dashForce * 2f, ForceMode.Impulse);

        // Nach kurzer Zeit abbremsen
        yield return new WaitForSeconds(0.1f);

        rb.linearVelocity = rb.linearVelocity * 0.2f; // Abbremsen, auf 20% der Geschwindigkeit

        yield return new WaitForSeconds(0.5f); // Delay bis DWGDestroyer Komponente wieder deaktiviert wird
        GetComponent<DWGDestroyer>().enabled = false;
        isDashing = false;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!isGrounded) CheckGroundStatus();
        PerformJump();
    }

    private void PerformJump()
    {
        if (isGrounded)
        {
            audioController.PlayJumpSound();
            rb.AddForce(gravityUp * jumpStrength, ForceMode.Impulse);
            isGrounded = false;
            canDoubleJump = playerManager.doubleJumpUnlocked;
        }
        else if (canDoubleJump)
        {
            audioController.PlayJumpSound();
            rb.AddForce(gravityUp * jumpStrength, ForceMode.Impulse);
            canDoubleJump = false;
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact!");
        currentInteractable?.Interact();

    }

    private void OnSwitchGravity(InputAction.CallbackContext ctx)
    {
        if (playerManager.gravitySwitchUnlocked)
        {
            GravityAreaFlat[] gravityAreas = FindObjectsByType<GravityAreaFlat>(FindObjectsSortMode.None);
            audioController.PlayGravitySound();
            foreach (var area in gravityAreas)
            {
                if (area.canBeFlipped)
                {
                    area.FlipGravityDirection();
                }
            }
            if (magnetControllerActive) magnetController.SwitchMagnetDirection();
        }
    }

    private void OnToggleMagnet(InputAction.CallbackContext ctx)
    {
        if (playerManager.gravitySwitchUnlocked)
        {
            if (magnetControllerActive)
            {
                magnetController.DisableAndClean();
                magnetControllerActive = false;
            } else
            {
                magnetController.gameObject.SetActive(true);
                magnetControllerActive = true;
            }
        }
    }

    // Alte Ground-Check Methode
    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, -gravityBody.GravityDirection) > 0.7f)
            {
                isGrounded = true;
                canDash = true;
                break;
            }
        }
    }

    /*private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, -gravityBody.GravityDirection) > 0.7f)
            {
                isGrounded = true;
                canDash = true;
                break;
            }
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable))
        {
            currentInteractable = interactable;
            interactable.ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable) && interactable == currentInteractable)
        {
            interactable.HidePrompt();
            currentInteractable = null;
        }
    }
}