using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushPullController : MonoBehaviour
{
    [Header("Grab settings")]
    public Camera playerCamera;
    public float grabDistance = 3f;
    public LayerMask grabLayerMask = ~0;

   // public float floorDistance = 0.2f;

    [Header("Joint settings")] //Un joint agafa es dos rigidbodies i els manté units
    public float breakForce = 1000f;
    public float breakTorque = 1000f;

    private Rigidbody playerRb;
    private FixedJoint currentJoint;
    private Rigidbody grabbedRb;

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

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        if (playerCamera == null) playerCamera = Camera.main;
        moveController = GetComponent<MoveCharacter>();
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

        // Crear joint para mostrar el empuje inicial
        currentJoint = gameObject.AddComponent<FixedJoint>();
        currentJoint.connectedBody = grabbedRb;
        currentJoint.breakForce = breakForce;
        currentJoint.breakTorque = breakTorque;

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
            // deixar de ser kinematic si fa falta
            grabbedRb = null;
        }

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
