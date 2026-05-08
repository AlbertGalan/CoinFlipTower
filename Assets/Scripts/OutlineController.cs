using UnityEngine;

public class OutlineController : MonoBehaviour
{
    [Header("Raycast settings")]
    [SerializeField] private float rayCastDistance = 4.0f;
    [SerializeField] private LayerMask raycastLayerMask;

    [Header("Layer names")]
    [SerializeField] private string raycastLayerName = "Raycast";
    [SerializeField] private string outlineLayerName = "Outline";

    [Header("Beam Settings")]
    [SerializeField] private Material hitMaterial;  // Material para cuando detecta algo
    [SerializeField] private Material missMaterial; // Material para cuando falla
    [SerializeField] private float beamThickness = 0.3f; 
    [SerializeField] private float beamLengthMultiplier = 6f; 
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject beamVisual;
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private GameObject blueWand;
    [SerializeField] private GameObject greenWand;
    
    private GameObject currentOutlinedObject;
    private int originalLayer;
    private Renderer beamRenderer; // Referencia al renderer del cilindro
    private Vector3 originalBeamScale;
    private bool lastHitState = false;
    private bool wandsVisible = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        SetupBeam();
    }

    private void SetupBeam()
    {
        if (beamVisual != null)
        {
            originalBeamScale = beamVisual.transform.localScale;
            beamRenderer = beamVisual.GetComponent<Renderer>();
            
            // Configurar layer para que siempre sea visible (opcional, según tu setup)
            beamVisual.layer = LayerMask.NameToLayer("UI"); 
            
            beamVisual.SetActive(false);
        }
        else
        {
            Debug.LogError("Beam Visual no asignado!");
        }
    }

    void Update()
    {
        HandleOutlineAndInteraction();
    }

    private void HandleOutlineAndInteraction()
    {
        bool shouldShowBeam = Input.GetKey(KeyCode.Mouse1); 
        
        if (beamVisual.activeSelf != shouldShowBeam)
        {
            beamVisual.SetActive(shouldShowBeam);
            
            if (!shouldShowBeam)
            {
                DisableCurrentOutline();
                HideAllWands();
                return;
            }
        }

        if (!shouldShowBeam) return;

        UpdateBeamPosition();

        if (Input.GetKeyDown(KeyCode.Mouse0) && currentOutlinedObject != null)
        {
            InteractWithCurrentObject();
        }
    }

    private void UpdateBeamPosition()
    {
        Vector3 rayOrigin = beamOrigin != null ? beamOrigin.position : playerCamera.transform.position;
        Vector3 cameraCenter = playerCamera.transform.position + playerCamera.transform.forward * rayCastDistance;
        Vector3 rayDirection = (cameraCenter - rayOrigin).normalized;

        RaycastHit hit;
        int raycastLayer = LayerMask.NameToLayer(raycastLayerName);
        int outlineLayer = LayerMask.NameToLayer(outlineLayerName);
        int combinedMask = (1 << raycastLayer) | (1 << outlineLayer);

        bool hasHit = Physics.Raycast(rayOrigin, rayDirection, out hit, rayCastDistance, combinedMask);
        float distance = hasHit ? hit.distance : rayCastDistance;

        PositionBeam(rayOrigin, rayDirection, distance);
        UpdateWandVisuals(hasHit);

        if (hasHit)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject != currentOutlinedObject)
            {
                DisableCurrentOutline();
                EnableOutline(hitObject);
            }
            SetBeamMaterial(hitMaterial); // Cambiar a material de impacto
        }
        else
        {
            DisableCurrentOutline();
            SetBeamMaterial(missMaterial); // Cambiar a material de fallo
        }
    }

    private void PositionBeam(Vector3 start, Vector3 direction, float distance)
    {
        Vector3 beamPosition = start + direction * (distance * 0.5f);

        beamVisual.transform.position = beamPosition;
        beamVisual.transform.rotation = Quaternion.LookRotation(direction);
        beamVisual.transform.Rotate(90f, 0f, 0f); 

        Vector3 newScale = originalBeamScale;
        newScale.y = (distance / 2f) * originalBeamScale.y * beamLengthMultiplier; 
        newScale.x = originalBeamScale.x * beamThickness; 
        newScale.z = originalBeamScale.z * beamThickness; 
        beamVisual.transform.localScale = newScale;
    }

    // Nueva función para cambiar el material
    private void SetBeamMaterial(Material newMat)
    {
        if (beamRenderer != null && newMat != null)
        {
            if (beamRenderer.sharedMaterial != newMat)
            {
                beamRenderer.material = newMat;
            }
        }
    }

    private void UpdateWandVisuals(bool hasHit)
    {
        if (wandsVisible && hasHit == lastHitState)
            return;

        lastHitState = hasHit;
        wandsVisible = true;

        if (hasHit)
        {
            if (greenWand != null) greenWand.SetActive(true);
            if (blueWand != null) blueWand.SetActive(false);
        }
        else
        {
            if (blueWand != null) blueWand.SetActive(true);
            if (greenWand != null) greenWand.SetActive(false);
        }
    }

    private void HideAllWands()
    {
        if (blueWand != null) blueWand.SetActive(false);
        if (greenWand != null) greenWand.SetActive(false);
        wandsVisible = false; 
    }

    private void EnableOutline(GameObject targetObject)
    {
        currentOutlinedObject = targetObject;
        originalLayer = targetObject.layer;
        targetObject.layer = LayerMask.NameToLayer(outlineLayerName);
    }

    private void DisableCurrentOutline()
    {
        if (currentOutlinedObject != null)
        {
            currentOutlinedObject.layer = originalLayer;
            currentOutlinedObject = null;
        }
    }

private void InteractWithCurrentObject()
{
    if (currentOutlinedObject != null)
    {
        // --- NUEVA LÓGICA EASTER EGG ---
        EasterEggBox eggBox = currentOutlinedObject.GetComponent<EasterEggBox>();
        if (eggBox != null)
        {
            eggBox.ActivateEasterEgg();
            return;
        }
        // 1. Verificar si es una casilla del Stroop
        StroopChoice stroop = currentOutlinedObject.GetComponent<StroopChoice>();
        if (stroop != null)
        {
            // Buscamos el manager en la escena y le enviamos la elección
            FindFirstObjectByType<StroopPuzzleManager>().OnPlayerClick(stroop);
            return; 
        }

        // 2. Tu lógica original de gravedad
        GravityController gravityObj = currentOutlinedObject.GetComponent<GravityController>();
        if (gravityObj != null)
        {
            gravityObj.ToggleGravity();
        }
    }
}
    
}