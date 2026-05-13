using UnityEngine;

public class BridgeGravityGear : MonoBehaviour
{
    [Header("Referencias")]
    public Transform bridgePivot;
    public MoveCharacter playerMove;
    public Transform gearRoot;

    [Header("Interacción")]
    public float interactionDistance = 5f;
    public LayerMask interactionMask = ~0;

    [Header("Configuración de Ángulos")]
    private float[] targetAngles = { -85f, -36f, 0f, 50f, 85f };
    private int currentStepIndex = 2; 

    [Header("Sensibilidad")]
    public float mouseDragThreshold = 0.2f; // Bajado un poco para que sea más reactivo
    public float transitionSpeed = 5f; 

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stepClip;
    [Range(0f,1f)] public float stepVolume = 1f;

    private bool isInteracting = false;
    private float mouseAccumulator = 0f;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) Debug.LogError("¡No se encuentra una cámara con el Tag 'MainCamera'!");

        if (gearRoot == null) gearRoot = transform;
        
        // Si no asignaste el playerMove manualmente, intentamos buscarlo
        if (playerMove == null) playerMove = FindFirstObjectByType<MoveCharacter>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleInteractionInput();
        ApplySmoothRotation();
    }

    private void HandleInteractionInput()
    {
        if (!isInteracting && Input.GetMouseButtonDown(0))
        {
            TryStartInteraction();
        }

        if (isInteracting)
        {
            if (Input.GetMouseButton(0))
            {
            float mouseX = Input.GetAxis("Mouse X");
            mouseAccumulator += mouseX;

            if (Mathf.Abs(mouseAccumulator) > mouseDragThreshold)
            {
                int prevIndex = currentStepIndex;
                if (mouseAccumulator > 0 && currentStepIndex < targetAngles.Length - 1)
                    currentStepIndex++;
                else if (mouseAccumulator < 0 && currentStepIndex > 0)
                    currentStepIndex--;

                if (prevIndex != currentStepIndex)
                {
                    PlayStepSound();
                }

                mouseAccumulator = 0;
            }
            }

            if (Input.GetMouseButtonUp(0))
                StopInteraction();
        }
    }

    private void TryStartInteraction()
    {
        if (!IsPlayerClickingGear())
            return;

        StartInteraction();
    }

    private void StartInteraction()
    {
        isInteracting = true;
        mouseAccumulator = 0;
        if (playerMove != null)
        {
            playerMove.SetBlockMovement(true);
            playerMove.SetLockCamera(true);
        }
        Debug.Log("Interacción iniciada con el engranaje");
    }

    private void StopInteraction()
    {
        isInteracting = false;
        if (playerMove != null)
        {
            playerMove.SetBlockMovement(false);
            playerMove.SetLockCamera(false);
        }
        Debug.Log("Interacción finalizada");
    }

    private void ApplySmoothRotation()
    {
        if (bridgePivot == null) return;

        float targetAngle = targetAngles[currentStepIndex];

        // Puente: Rotación en Y
        Quaternion bridgeTargetRot = Quaternion.Euler(0, targetAngle, 0);
        bridgePivot.localRotation = Quaternion.Slerp(bridgePivot.localRotation, bridgeTargetRot, Time.deltaTime * transitionSpeed);

        // Engranaje: Rotación en Z
        Quaternion gearTargetRot = Quaternion.Euler(0, 0, targetAngle); 
        transform.localRotation = Quaternion.Slerp(transform.localRotation, gearTargetRot, Time.deltaTime * transitionSpeed);
    }

    private bool IsPlayerClickingGear()
    {
        if (mainCam == null) return false;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask))
            return false;

        return IsPartOfGear(hit.transform);
    }

    private bool IsPartOfGear(Transform hitTransform)
    {
        if (gearRoot == null)
            return false;

        return hitTransform == gearRoot
            || hitTransform.IsChildOf(gearRoot)
            || gearRoot.IsChildOf(hitTransform);
    }

    private void PlayStepSound()
    {
        if (audioSource != null && stepClip != null)
        {
            audioSource.PlayOneShot(stepClip, stepVolume);
        }
    }
}