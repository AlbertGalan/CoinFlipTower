using UnityEngine;

public class BlockSpawnerLeverController : MonoBehaviour
{
    public enum BlockColorType { Blue, Red }

    [Header("Block Configuration")]
    public BlockColorType colorToSpawn; // Selecciona Blue o Red en el Inspector
    public GameObject blockPrefab;     // Arrastra el prefab correspondiente
    public Transform spawnPoint;

    [Header("Scene Tracking (Read Only)")]
    [SerializeField] private GameObject currentBlock;

    [Header("References")]
    public Camera playerCamera;
    public MoveCharacter moveCharacter;
    public Transform leverVisual;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public LayerMask interactionMask = ~0;

    [Header("Lever Angles")]
    public float restAngleX = -30f;
    public float pulledAngleX = 30f;
    public float pullSensitivity = 6f;
    public float returnSpeed = 90f;
    public float pullTriggerTolerance = 0.5f;

    private float currentAngleX;
    private bool isHolding;
    private bool spawnTriggeredThisHold;
    private float fixedLocalY;
    private float fixedLocalZ;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (leverVisual == null) leverVisual = transform;
        if (moveCharacter == null) moveCharacter = FindFirstObjectByType<MoveCharacter>();

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
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask))
        {
            if (IsPartOfLever(hit.collider.transform))
            {
                isHolding = true;
                spawnTriggeredThisHold = false;

                if (moveCharacter != null)
                {
                    moveCharacter.SetLockCamera(true);
                    moveCharacter.SetBlockMovement(true);
                }
            }
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

        if (!spawnTriggeredThisHold && IsFullyPulled())
        {
            spawnTriggeredThisHold = true;
            SpawnBlock();
        }
    }

    private void SpawnBlock()
    {
        // Solo spawnea si el bloque anterior fue destruido (la referencia es null)
        if (currentBlock == null && blockPrefab != null)
        {
            currentBlock = Instantiate(blockPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"Spawneado bloque de color: {colorToSpawn}");
        }
        else
        {
            Debug.Log($"Ya hay un bloque {colorToSpawn} activo. Destrúyelo para spawnear otro.");
        }
    }

    private void ReleaseLever()
    {
        if (!spawnTriggeredThisHold && IsFullyPulled())
        {
            spawnTriggeredThisHold = true;
            SpawnBlock();
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
        if (Mathf.Abs(currentAngleX - restAngleX) < 0.001f) return;
        currentAngleX = Mathf.MoveTowards(currentAngleX, restAngleX, returnSpeed * Time.deltaTime);
        ApplyLeverRotation(currentAngleX);
    }

    private bool IsPartOfLever(Transform hitTransform)
    {
        return hitTransform == leverVisual || hitTransform.IsChildOf(leverVisual) || leverVisual.IsChildOf(hitTransform);
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
        if (isHolding) ReleaseLever();
    }
}