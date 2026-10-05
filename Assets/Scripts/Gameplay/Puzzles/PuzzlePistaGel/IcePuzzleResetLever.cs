using UnityEngine;

public class IcePuzzleResetLever : MonoBehaviour
{
    [Header("References")]
    public GameObject iceBlockPrefab;      // Arrastra el PREFAB del bloque desde tu carpeta Project
    public GameObject currentBlockInstance; // La instancia que está actualmente en la escena
    public Transform spawnPoint;          
    public Camera playerCamera;
    public MoveCharacter moveCharacter;   
    public Transform leverVisual;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public float pulledAngleX = 30f;
    public float restAngleX = -30f;
    public float pullSensitivity = 6f;
    public float returnSpeed = 90f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip resetSound;

    private float currentAngleX;
    private bool isHolding;
    private bool triggered;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        currentAngleX = restAngleX;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryBeginHold();

        if (isHolding)
        {
            if (Input.GetMouseButton(0)) UpdatePull();
            if (Input.GetMouseButtonUp(0)) ReleaseLever();
        }
        else
        {
            ReturnToRest();
        }
    }

    private void TryBeginHold()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.transform == leverVisual || hit.transform.IsChildOf(leverVisual))
            {
                isHolding = true;
                triggered = false;
                if (moveCharacter != null) {
                    moveCharacter.SetLockCamera(true);
                    moveCharacter.SetBlockMovement(true);
                }
            }
        }
    }

    private void UpdatePull()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        currentAngleX = Mathf.Clamp(currentAngleX - mouseY * pullSensitivity, Mathf.Min(restAngleX, pulledAngleX), Mathf.Max(restAngleX, pulledAngleX));
        leverVisual.localRotation = Quaternion.Euler(currentAngleX, 0, 0);

        if (!triggered && Mathf.Abs(currentAngleX - pulledAngleX) < 5f)
        {
            triggered = true;
            ResetBlock();
        }
    }

    private void ResetBlock()
    {
        if (iceBlockPrefab != null && spawnPoint != null)
        {
            Debug.Log("<color=blue>Ice Puzzle:</color> Reemplazando bloque por una nueva instancia...");

            // 1. Destruir el bloque actual si existe
            if (currentBlockInstance != null)
            {
                Destroy(currentBlockInstance);
            }

            // 2. Instanciar el nuevo bloque desde el Prefab
            currentBlockInstance = Instantiate(iceBlockPrefab, spawnPoint.position, spawnPoint.rotation);

            // 3. Configurar Gravedad Invertida en la nueva instancia
            var gravityCtrl = currentBlockInstance.GetComponent<GravityController>();
            if (gravityCtrl != null)
            {
                gravityCtrl.gravityInverted = true;
            }

            // 4. Sonido
            if (audioSource != null && resetSound != null)
                audioSource.PlayOneShot(resetSound);
        }
    }

    private void ReleaseLever()
    {
        isHolding = false;
        if (moveCharacter != null) {
            moveCharacter.SetLockCamera(false);
            moveCharacter.SetBlockMovement(false);
        }
    }

    private void ReturnToRest()
    {
        currentAngleX = Mathf.MoveTowards(currentAngleX, restAngleX, returnSpeed * Time.deltaTime);
        leverVisual.localRotation = Quaternion.Euler(currentAngleX, 0, 0);
    }
}