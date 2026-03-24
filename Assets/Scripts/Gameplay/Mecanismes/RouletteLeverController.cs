using UnityEngine;

public class RouletteLeverController : MonoBehaviour
{
    [Header("References")]
    public RouletteWheel rouletteWheel;
    public Camera playerCamera;
    [Tooltip("Optional: lock camera/movement while dragging the lever")]
    public MoveCharacter moveCharacter;
    [Tooltip("Transform that visually rotates as lever (defaults to this transform)")]
    public Transform leverVisual;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public LayerMask interactionMask = ~0;

    [Header("Lever Angles")]
    [Tooltip("Rest angle (local X)")]
    public float restAngleX = -30f;
    [Tooltip("Pulled angle (local X)")]
    public float pulledAngleX = 30f;
    [Tooltip("Degrees added per mouse Y unit while holding click")]
    public float pullSensitivity = 6f;
    [Tooltip("How fast the lever returns to rest after release")]
    public float returnSpeed = 90f;
    [Tooltip("Tolerance to consider the lever fully pulled")]
    public float pullTriggerTolerance = 0.5f;

    private float currentAngleX;
    private bool isHolding;
    private bool spinTriggeredThisHold;
    private float fixedLocalY;
    private float fixedLocalZ;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (leverVisual == null)
            leverVisual = transform;

        if (moveCharacter == null)
            moveCharacter = FindFirstObjectByType<MoveCharacter>();

        Vector3 initialLocalEuler = leverVisual.localEulerAngles;
        fixedLocalY = initialLocalEuler.y;
        fixedLocalZ = initialLocalEuler.z;

        currentAngleX = restAngleX;
        ApplyLeverRotation(currentAngleX);
    }

    private void Update()
    {
        if (!isHolding && Input.GetMouseButtonDown(0))
        {
            TryBeginHold();
        }

        if (isHolding)
        {
            if (Input.GetMouseButton(0))
            {
                UpdatePullFromMouse();
            }

            if (Input.GetMouseButtonUp(0))
            {
                ReleaseLever();
            }
        }
        else
        {
            ReturnToRest();
        }
    }

    private void TryBeginHold()
    {
        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask))
            return;

        if (!IsPartOfLever(hit.collider.transform))
            return;

        isHolding = true;
        spinTriggeredThisHold = false;

        if (moveCharacter != null)
        {
            moveCharacter.SetLockCamera(true);
            moveCharacter.SetBlockMovement(true);
        }
    }

    private void UpdatePullFromMouse()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        float deltaX = -mouseY * pullSensitivity;

        float minAngle = Mathf.Min(restAngleX, pulledAngleX);
        float maxAngle = Mathf.Max(restAngleX, pulledAngleX);

        currentAngleX = Mathf.Clamp(currentAngleX + deltaX, minAngle, maxAngle);
        ApplyLeverRotation(currentAngleX);

        if (!spinTriggeredThisHold && IsFullyPulled())
        {
            spinTriggeredThisHold = true;
            if (rouletteWheel != null)
                rouletteWheel.Spin();
        }
    }

    private void ReleaseLever()
    {
        if (!spinTriggeredThisHold && IsFullyPulled())
        {
            spinTriggeredThisHold = true;
            if (rouletteWheel != null)
                rouletteWheel.Spin();
        }

        isHolding = false;

        if (moveCharacter != null)
        {
            moveCharacter.SetLockCamera(false);
            moveCharacter.SetBlockMovement(false);
        }
    }

    private void ReturnToRest()
    {
        if (Mathf.Abs(currentAngleX - restAngleX) < 0.001f)
            return;

        currentAngleX = Mathf.MoveTowards(currentAngleX, restAngleX, returnSpeed * Time.deltaTime);
        ApplyLeverRotation(currentAngleX);
    }

    private bool IsPartOfLever(Transform hitTransform)
    {
        return hitTransform == leverVisual
            || hitTransform.IsChildOf(leverVisual)
            || leverVisual.IsChildOf(hitTransform);
    }

    private bool IsFullyPulled()
    {
        if (pulledAngleX >= restAngleX)
            return currentAngleX >= pulledAngleX - pullTriggerTolerance;

        return currentAngleX <= pulledAngleX + pullTriggerTolerance;
    }

    private void ApplyLeverRotation(float localX)
    {
        leverVisual.localRotation = Quaternion.Euler(localX, fixedLocalY, fixedLocalZ);
    }

    private void OnDisable()
    {
        if (isHolding)
            ReleaseLever();
    }
}
