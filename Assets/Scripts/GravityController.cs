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

    [Header("Grab Block")]
    [Tooltip("Temps (segons) que es bloqueja l'agafar després d'invertir gravetat")]
    public float grabBlockDuration = 0.75f;

    [Header("Gravity Lock")]
    [Tooltip("Bloqueja nous canvis de gravetat després del primer canvi")]
    public bool lockAfterFirstGravityChange = false;

    [Header("Anti-Spam")]
    [Tooltip("Si està activat, el jugador ha de tocar terra o sostre abans de tornar a invertir la gravetat")]
    public bool requireContactBetweenGravityToggles = true;

    [Header("Configuració per a l'stick del jugador")]
    [Tooltip("Màxima distància per cercar un sostre quan s'intenta enganxar")] public float stickMaxDistance = 1.5f;
    [Tooltip("Layermask utilitzada per detectar sostres per enganxar-se")] public LayerMask stickLayerMask = ~0;

    private Rigidbody rb;
    private Vector3 currentGravityDirection;
    private PushPullController pushPull;

    private Quaternion initialLocalRotation;
    private float lastYaw;

    private float lastGravityToggleTime = -999f;
    private bool gravityChangeLocked = false;
    private bool canChangeGravityByContact = true;

    // IsStuck estat (utilitzat quan el jugador s'enganxa al sostre)
    public bool IsStuck { get; private set; } = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;

        initialLocalRotation = transform.localRotation;
        lastYaw = transform.localEulerAngles.y;

        UpdateGravityDirection();
        ApplyVisualRotation(false);

        // cache PushPullController if present to check grabbing state
        pushPull = GetComponent<PushPullController>();
    }

    void Update()
    {
        if (!isPlayer) return;

        // Bloquejar la inversió de gravetat quan el joc està pausat
        if (Time.timeScale == 0f) return;

        // Bloquejar la inversió de gravetat mentre es mostra un diàleg del tutorial
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsShowingMessage())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // If the player is currently grabbing an object, ignore gravity toggle
            if (pushPull != null && pushPull.IsGrabbing)
            {
                return;
            }

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
        if (gravityChangeLocked)
            return;

        // Evita spam: cal tornar a tocar terra o sostre abans de poder invertir de nou.
        if (isPlayer && requireContactBetweenGravityToggles && !canChangeGravityByContact)
            return;

        gravityInverted = !gravityInverted;
        UpdateGravityDirection();
        lastGravityToggleTime = Time.time;
        if (isPlayer && requireContactBetweenGravityToggles)
            canChangeGravityByContact = false;

        // Crude snap: when player flips gravity, adjust Y by +/-2 to avoid clipping
        if (isPlayer && rb != null && !IsStuck)
        {
            Vector3 pos = rb.position;
            if (gravityInverted)
            {
                pos.y += 2f;
            }
            else
            {
                pos.y -= 2f;
            }
            rb.MovePosition(pos);
        }

        ApplyVisualRotation(true);

        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.LogGravityFlip(isPlayer);
        }

        if (lockAfterFirstGravityChange)
            gravityChangeLocked = true;
    }

    public void SetGravityInverted(bool inverted)
    {
        if (gravityChangeLocked)
            return;

        if (isPlayer && requireContactBetweenGravityToggles && !canChangeGravityByContact)
            return;

        if (gravityInverted == inverted) return;
        gravityInverted = inverted;
        UpdateGravityDirection();
        lastGravityToggleTime = Time.time;
        if (isPlayer && requireContactBetweenGravityToggles)
            canChangeGravityByContact = false;
        // Crude snap as above
        if (isPlayer && rb != null && !IsStuck)
        {
            Vector3 pos = rb.position;
            if (gravityInverted)
            {
                pos.y += 2f;
            }
            else
            {
                pos.y -= 2f;
            }
            rb.MovePosition(pos);
        }

        ApplyVisualRotation(true);

        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.LogGravityFlip(isPlayer);
        }

        if (lockAfterFirstGravityChange)
            gravityChangeLocked = true;
    }

    public void UpdateGravityDirection()
    {
        currentGravityDirection = gravityInverted ? Vector3.up : Vector3.down;
    }

    public bool IsGravityInverted()
    {
        return gravityInverted;
    }

    public bool CanBeGrabbed()
    {
        return Time.time - lastGravityToggleTime >= grabBlockDuration;
    }

    public bool IsGravityChangeLocked()
    {
        return gravityChangeLocked;
    }

    public void LockGravityChanges()
    {
        gravityChangeLocked = true;
    }

    public void UnlockGravityChanges()
    {
        gravityChangeLocked = false;
    }

    private void ApplyVisualRotation(bool smooth)
    {
        if (!rotateOnInvert) return;

        // Guardar la direcció actual (yaw) i mantenir-la
        lastYaw = transform.localEulerAngles.y;

        // Forçar X a 0 per evitar inclinacions i només girar Z
        float z = gravityInverted ? 180f : 0f;
        Quaternion target = Quaternion.Euler(0f, lastYaw, z);
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

    private void OnCollisionEnter(Collision collision)
    {
        MarkGravityContactIfApplicable(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        MarkGravityContactIfApplicable(collision);
    }

    private void MarkGravityContactIfApplicable(Collision collision)
    {
        if (!isPlayer || !requireContactBetweenGravityToggles || canChangeGravityByContact || collision == null)
            return;

        // Considerem "contacte vàlid" només si és una superfície vertical (terra/sostre).
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (Mathf.Abs(Vector3.Dot(normal, Vector3.up)) >= 0.5f)
            {
                canChangeGravityByContact = true;
                return;
            }
        }
    }

    private void OnValidate()
    {
        // Si es desactiva el control anti-spam, no bloqueamos más cambios por contacto.
        if (!requireContactBetweenGravityToggles)
        {
            canChangeGravityByContact = true;
        }
    }
}