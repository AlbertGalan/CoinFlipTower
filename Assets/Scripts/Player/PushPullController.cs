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

    private Rigidbody playerRb;
    private FixedJoint currentJoint;
    private Rigidbody grabbedRb;
    private bool isGrabbing = false;
    // Per detectar si s'està agafant un objecte
    public bool IsGrabbing => isGrabbing;

    private MoveCharacter moveController;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        if (playerCamera == null) playerCamera = Camera.main;
        moveController = GetComponent<MoveCharacter>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!isGrabbing)
            {
                TryGrab();
            }
            else
            {
                Release();
            }
        }
    }

    private void TryGrab()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, grabDistance, grabLayerMask))
        {
            Rigidbody targetRb = hit.collider.attachedRigidbody;
            if (targetRb != null && targetRb != playerRb)
            {
                Grab(targetRb, hit.point);
            }
        }
    }

    private void Grab(Rigidbody targetRb, Vector3 hitPoint)
    {
        grabbedRb = targetRb;

        // Assignar kinematic per evitar que l'objecte caigui mentre s'agafa
        if (grabbedRb.isKinematic)
            grabbedRb.isKinematic = false;

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
    }

    private void OnDestroy()
    {
        Release();
    }
}
