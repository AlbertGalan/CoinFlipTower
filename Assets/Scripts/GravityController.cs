using UnityEngine;

public class GravityController : MonoBehaviour
{
    [Header("Gravity")]
    public float gravityForce = 9.81f;
    public bool gravityInverted = false;

    [Header("Behavior")]
    [Tooltip("Pitjar això si vull que ho detecti com a jugador.")]
    public bool isPlayer = false;

    [Tooltip("Si es vol que l'objecte/jugador giri quan s'inverteix la gravetat")]
    public bool rotateOnInvert = false;

    [Header("Configuració per a l'stick del jugador")]
    [Tooltip("Màxima distància per cercar un sostre quan s'intenta enganxar")] public float stickMaxDistance = 1.5f;
    [Tooltip("Layermask utilitzada per detectar sostres per enganxar-se")] public LayerMask stickLayerMask = ~0;

    private Rigidbody rb;
    private Vector3 currentGravityDirection;

    private Quaternion initialLocalRotation;
    private Quaternion invertedLocalRotation;

    // IsStuck estat (utilitzat quan el jugador s'enganxa al sostre)
    public bool IsStuck { get; private set; } = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;

        initialLocalRotation = transform.localRotation;
        invertedLocalRotation = initialLocalRotation * Quaternion.Euler(180f, 0f, 0f);

        UpdateGravityDirection();
        ApplyVisualRotation(false);
    }

    void Update()
    {
        if (!isPlayer) return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGravity();

            if (gravityInverted)
            {
                TryStickToCeiling();
            }
            else
            {
                ReleaseStick();
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (IsStuck) return; // No aplicar gravetat si està enganxat

        if (rb.isKinematic)
        {
            Vector3 delta = currentGravityDirection * gravityForce * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + delta);
        }
        else
        {
            rb.AddForce(currentGravityDirection * gravityForce, ForceMode.Acceleration);
        }
    }

    public void ToggleGravity()
    {
        gravityInverted = !gravityInverted;
        UpdateGravityDirection();
        ApplyVisualRotation(true);
    }

    public void SetGravityInverted(bool inverted)
    {
        if (gravityInverted == inverted) return;
        gravityInverted = inverted;
        UpdateGravityDirection();
        ApplyVisualRotation(true);
    }

    public void UpdateGravityDirection()
    {
        currentGravityDirection = gravityInverted ? Vector3.up : Vector3.down;
    }

    public bool IsGravityInverted()
    {
        return gravityInverted;
    }

    private void ApplyVisualRotation(bool smooth)
    {
        if (!rotateOnInvert) return;

        Quaternion target = gravityInverted ? invertedLocalRotation : initialLocalRotation;
        transform.localRotation = target;
    }

    // --- Sticking API (player) ---
    private void TryStickToCeiling()
    {
        if (rb == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.up, out hit, stickMaxDistance, stickLayerMask))
        {
            float offset = 1f;
            Collider c = GetComponent<Collider>();
            if (c != null) offset = c.bounds.extents.y;

            Vector3 targetPos = hit.point - hit.normal * (offset + 0.05f);

            StickToPosition(targetPos);
        }
        else
        {
            // no hi ha sostre a prop
        }
    }

    public void StickToPosition(Vector3 worldPosition)
    {
        if (rb == null) return;

        // aturar moviment i fer kinematic per poder mantenir la posició
        #if UNITY_2023_1_OR_NEWER
        rb.linearVelocity = Vector3.zero;
        #else
        rb.velocity = Vector3.zero;
        #endif
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.MovePosition(worldPosition);
        IsStuck = true;
        ApplyVisualRotation(false);
    }

    public void ReleaseStick()
    {
        if (rb == null) return;

        rb.isKinematic = false;
        IsStuck = false;
        ApplyVisualRotation(false);
    }
}