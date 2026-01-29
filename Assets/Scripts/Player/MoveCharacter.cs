using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveCharacter : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 90f;

    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    private Rigidbody rb;
    [HideInInspector] public bool lockCamera = false;
    [HideInInspector] public bool restrictStrafe = false;
    [Header("Collision/Push settings")]
    [Tooltip("Triar les capes que bloquejaran el moviment del jugador quan no estigui agafant res")]
    public LayerMask nonPushLayerMask = 0;
    [Tooltip("Radius emprat per la comprovació d'obstacles quan el jugador es mou")]
    public float obstacleCheckRadius = 0.4f;
    [Header("Animation")]
    [Tooltip("Component d'animacions")]
    private Animator animator;
    private float inputH = 0f;
    private float inputV = 0f;
    [Tooltip("Nom del paràmetre float emprat per la blend tree d'animacions")]
    public string blendParameter = "Blend";

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Auto-assign animator if not set in inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator == null)
                Debug.LogWarning("MoveCharacter: No s'han trobat animacions.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        // use the inputs read in Update to keep animation and movement consistent
        // (inputH and inputV are set in Update)

        if (restrictStrafe)
        {
            // only allow forward/back (no strafing)
            inputH = 0f;
        }

        Vector3 moveDirection = (transform.forward * inputV + transform.right * inputH).normalized;

        float moveDistance = moveSpeed * Time.fixedDeltaTime;

        // If there's an obstacle in the nonPushLayerMask in our movement direction and
        // the player is NOT currently grabbing (no FixedJoint), block movement to avoid pushing.
        bool canMove = true;
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            // skip check if there is a FixedJoint (we are grabbing)
            FixedJoint fj = GetComponent<FixedJoint>();
            if (fj == null && nonPushLayerMask != (LayerMask)0)
            {
                Vector3 sphereOrigin = rb.position + Vector3.up * 0.5f; // approximate player center
                RaycastHit hit;
                if (Physics.SphereCast(sphereOrigin, obstacleCheckRadius, moveDirection, out hit, moveDistance + 0.05f, nonPushLayerMask))
                {
                    // Found an obstacle in the disallowed mask — block movement
                    canMove = false;
                }
            }
        }

        if (canMove)
        {
            Vector3 newPosition = rb.position + moveDirection * moveDistance;
            rb.MovePosition(newPosition);
        }
    }

    void Update()
    {
        // --- Rotació ratolí ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // read movement input here so we can update the animator immediately
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");

        // Update animator parameters. You configured a 1D blend tree using a single
        // float parameter with thresholds: Idle=0, Forward=0.25, Back=0.5, Left=0.75, Right=1.
        if (animator != null)
        {
            float blendValue = 0f;

            // Determine dominant input to decide which directional animation to play.
            Vector2 inputVec = new Vector2(inputH, inputV);
            if (inputVec.sqrMagnitude <= 0.001f)
            {
                blendValue = 0f; // Idle
            }
            else
            {
                // choose the axis with larger absolute value
                if (Mathf.Abs(inputV) >= Mathf.Abs(inputH))
                {
                    // Forward / Back
                    blendValue = inputV > 0f ? 0.25f : 0.5f;
                }
                else
                {
                    // Right / Left (note: your thresholds had Left=0.75, Right=1)
                    blendValue = inputH > 0f ? 1f : 0.75f;
                }
            }

            // Smoothly set the blend parameter
            animator.SetFloat(blendParameter, blendValue, 0.08f, Time.deltaTime);

         
        }

        if (!lockCamera)
        {
            //rotar el personatge (solo si la cámara no está bloqueada)
            transform.Rotate(Vector3.up * mouseX);

            // Rotar camara verticalment amb limitacions
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 60f);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
        else
        {
            // cuando la cámara está bloqueada no actualizamos la rotación vertical
        }
    }

    // API para que otros controladores cambien el comportamiento
    public void SetLockCamera(bool locked)
    {
        lockCamera = locked;
    }

    public void SetRestrictStrafe(bool restrict)
    {
        restrictStrafe = restrict;
    }
}
