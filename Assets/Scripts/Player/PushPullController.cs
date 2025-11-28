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

        // Ensure target is non-kinematic so joint can move it
        if (grabbedRb.isKinematic)
            grabbedRb.isKinematic = false;

        currentJoint = gameObject.AddComponent<FixedJoint>();
        currentJoint.connectedBody = grabbedRb;
        currentJoint.breakForce = breakForce;
        currentJoint.breakTorque = breakTorque;

        isGrabbing = true;

        // Lock camera and restrict strafing
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
            // leave kinematic as false (object returns to physics)
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
