using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushPullController : MonoBehaviour
{
    [Header("Grab settings")]
    public Camera playerCamera;
    public float grabDistance = 3f;
    public LayerMask grabLayerMask = ~0;

    [Header("Joint settings")] //Un joint agafa es dos rigidbodies i els manté units
    public float breakForce = 1000f;
    public float breakTorque = 1000f;

    [Header("Configurable Joint settings")]
    [Tooltip("Distancia maxima permitida entre el jugador y el objeto agarrado")]
    public float grabMaxDistance = 0.2f;
    [Tooltip("Rigidez del muelle del joint (alto = agarre mas firme)")]
    public float jointSpring = 5000f;
    [Tooltip("Amortiguacion del muelle del joint")]
    public float jointDamper = 500f;
    
    [Header("Mass amplification")]
    [Tooltip("Multiplicador de masa del jugador mientras agarra (para empujar objetos pesados)")]
    //public float massMultiplier = 5f;
    //[Tooltip("Aplicar fuerza extra al objeto cuando el jugador se mueve")]
    public bool applyPushForce = true;
    [Tooltip("Multiplicador de la fuerza de empuje")]
    public float pushForceMultiplier = 2f;

    private Rigidbody playerRb;
    private ConfigurableJoint currentJoint;
    private Rigidbody grabbedRb;
    private float originalPlayerMass;

   // private Vector3 originalGrabbedPosition;
    private bool isGrabbing = false;
    // Per detectar si s'està agafant un objecte
    public bool IsGrabbing => isGrabbing;

    private MoveCharacter moveController;
    // Proximity highlight
    [Header("Visual feedback")]
    [Tooltip("Activar el highlight visual quan s'està a prop d'objectes agafables")]
    public bool enableProximityHighlight = true;
    [Tooltip("Distància màxima a la qual els objectes mostraran el highlight de proximitat")]
    public float highlightDistance = 2f;

    private GrabbableVisual currentProximityVisual;

    [Header("Curved push settings")]
    [Tooltip("Fuerza base de empuje hacia adelante")]
    public float pushForceForward = 500f;
    [Tooltip("Fuerza lateral aplicada al empujar en diagonal")]
    public float pushForceLateral = 300f;
    [Tooltip("Suavidad de la curva (0-1, menor = más suave)")]
    public float curveSmoothing = 0.3f;

    private Vector3 lastPushDirection = Vector3.forward;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        if (playerCamera == null) playerCamera = Camera.main;
        moveController = GetComponent<MoveCharacter>();
        originalPlayerMass = playerRb.mass;
    }

    void FixedUpdate()
    {
        // Aplicar fuerza extra al objeto cuando el jugador se mueve mientras lo agarra
        if (isGrabbing && grabbedRb != null && applyPushForce)
        {
            Vector3 playerVelocity = playerRb.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
            
            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                // Aplicar fuerza en la dirección del movimiento del jugador
                grabbedRb.AddForce(horizontalVelocity * pushForceMultiplier * grabbedRb.mass, ForceMode.Force);
            }
        }
    }

    void Update()
    {
        // Mantenir pitjant el botó d'agafar
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            TryGrab();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isGrabbing)
                Release();
        }

        // Mostrar highlight de proximitat (no quan està actiu el click dret)
        if (enableProximityHighlight && !isGrabbing && !Input.GetMouseButton(1))
        {
            UpdateProximityHighlight();
        }
        else if (Input.GetMouseButton(1) && currentProximityVisual != null)
        {
            currentProximityVisual.Highlight(false);
            currentProximityVisual = null;
        }
    }

    private void TryGrab()
    {
        if (isGrabbing) return; // ja s'està agafant

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, grabDistance, grabLayerMask))
        {
            Rigidbody targetRb = hit.collider.attachedRigidbody;
            if (targetRb != null && targetRb != playerRb)
            {
                // Bloquejar l'agafar uns instants després d'invertir la gravetat
                GravityController gravityObj = targetRb.GetComponent<GravityController>();
                if (gravityObj != null && !gravityObj.CanBeGrabbed())
                {
                    return;
                }

                Grab(targetRb, hit.point);
            }
        }
    }

    private void UpdateProximityHighlight()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, highlightDistance, grabLayerMask))
        {
            Rigidbody targetRb = hit.collider.attachedRigidbody;
            if (targetRb != null && targetRb != playerRb)
            {
                var visual = targetRb.GetComponentInChildren<GrabbableVisual>();
                if (visual != null)
                {
                    if (currentProximityVisual != visual)
                    {
                        if (currentProximityVisual != null) currentProximityVisual.Highlight(false);
                        currentProximityVisual = visual;
                        currentProximityVisual.Highlight(true);
                    }
                    return;
                }
            }
        }

        // res agafable a prop
        if (currentProximityVisual != null)
        {
            currentProximityVisual.Highlight(false);
            currentProximityVisual = null;
        }
    }

    private void Grab(Rigidbody targetRb, Vector3 hitPoint)
    {
        grabbedRb = targetRb;

        // Assignar kinematic per evitar que l'objecte caigui mentre s'agafa
        if (grabbedRb.isKinematic)
            grabbedRb.isKinematic = false;

        // Amplificar masa del jugador para poder empujar objetos pesados
       // playerRb.mass = originalPlayerMass * massMultiplier;

        // ConfigurableJoint para un agarre mas flexible
        currentJoint = gameObject.AddComponent<ConfigurableJoint>();
        currentJoint.autoConfigureConnectedAnchor = false;
        currentJoint.connectedBody = grabbedRb;
        currentJoint.anchor = transform.InverseTransformPoint(hitPoint);
        currentJoint.connectedAnchor = grabbedRb.transform.InverseTransformPoint(hitPoint);
        currentJoint.breakForce = breakForce;
        currentJoint.breakTorque = breakTorque;

        currentJoint.xMotion = ConfigurableJointMotion.Limited;
        currentJoint.yMotion = ConfigurableJointMotion.Limited;
        currentJoint.zMotion = ConfigurableJointMotion.Limited;
        currentJoint.angularXMotion = ConfigurableJointMotion.Locked;
        currentJoint.angularYMotion = ConfigurableJointMotion.Locked;
        currentJoint.angularZMotion = ConfigurableJointMotion.Locked;

        SoftJointLimit linearLimit = new SoftJointLimit();
        linearLimit.limit = grabMaxDistance;
        currentJoint.linearLimit = linearLimit;

        JointDrive drive = new JointDrive();
        drive.positionSpring = jointSpring;
        drive.positionDamper = jointDamper;
        drive.maximumForce = Mathf.Infinity;
        currentJoint.xDrive = drive;
        currentJoint.yDrive = drive;
        currentJoint.zDrive = drive;

        isGrabbing = true;

        // Bloquejar càmera mentre s'agafa
        if (moveController != null)
        {
            moveController.SetLockCamera(true);
            moveController.SetRestrictStrafe(true);
        }

        // Canviar a càmara cenital (top-down) per facilitar la visualització de l'empenta/estirada (prototip)
        if (CameraSwitcher.Instance != null)
        {
            CameraSwitcher.Instance.SetTopDown();
        }
    }

    private void Release()
    {
        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }

        if (grabbedRb != null)
        {
            // Reducir velocidad del objeto al soltar para evitar que salga disparado
            Vector3 vel = grabbedRb.linearVelocity;
            grabbedRb.linearVelocity = vel * 0.5f;
            grabbedRb.angularVelocity *= 0.5f;
            
            grabbedRb = null;
        }

        // Restaurar masa original del jugador
        playerRb.mass = originalPlayerMass;

        isGrabbing = false;

        if (moveController != null)
        {
            moveController.SetLockCamera(false);
            moveController.SetRestrictStrafe(false);
        } 

        // Tornar a la càmera en primera persona al deixar anar
        if (CameraSwitcher.Instance != null)
        {
            CameraSwitcher.Instance.SetFirstPerson();
        }
    }

    private void OnDestroy()
    {
        Release();
    }
}
