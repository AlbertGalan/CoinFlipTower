using UnityEngine;

public class OutlineController : MonoBehaviour
{
    [Header("Raycast settings")]
    [SerializeField] private float rayCastDistance = 4.0f;
    [SerializeField] private LayerMask raycastLayerMask;

    [Header("Layer names")]
    [SerializeField] private string raycastLayerName = "Raycast";
    [SerializeField] private string outlineLayerName = "Outline";

   // [Header("Input")]
   // [SerializeField] private KeyCode outlineKey = KeyCode.Mouse1;

    [Header("Beam Settings")]
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.cyan;
    [SerializeField] [Range(0f, 1f)] private float beamOpacity = 1f; // Posar a 1 temporalment
    [SerializeField] private float beamThickness = 0.3f; // Grosor del beam (escala X y Z)
    [SerializeField] private float beamLengthMultiplier = 6f; // Multiplicador de longitud del beam
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject beamVisual;
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private GameObject blueWand;
    [SerializeField] private GameObject greenWand;
    
    private GameObject currentOutlinedObject;
    private int originalLayer;
    private Material beamMaterial;
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
            
            Renderer beamRenderer = beamVisual.GetComponent<Renderer>();
            if (beamRenderer != null)
            {
                beamMaterial = new Material(beamRenderer.material);
                beamRenderer.material = beamMaterial;
                SetupMaterialForBeam();
            }
            
            // Configurar layer para que siempre sea visible
            beamVisual.layer = LayerMask.NameToLayer("UI"); // O una layer que siempre se vea
            
            beamVisual.SetActive(false);
            
            Debug.Log("Beam configurado. Escala original: " + originalBeamScale);
        }
        else
        {
            Debug.LogError("Beam Visual no asignado!");
        }
    }

    private void SetupMaterialForBeam()
    {
        // Usar un shader simple siempre visible
        beamMaterial.shader = Shader.Find("Unlit/Color");
        beamMaterial.color = new Color(hitColor.r, hitColor.g, hitColor.b, beamOpacity);
        
        // Configurar para que ignore iluminación y sombras
        beamMaterial.SetFloat("_Glossiness", 0f);
        beamMaterial.SetFloat("_Metallic", 0f);
    }

    void Update()
    {
        HandleOutlineAndInteraction();
    }

    private void HandleOutlineAndInteraction()
    {
        bool shouldShowBeam = Input.GetKey(KeyCode.Mouse1); // Click dret per mostrar el beam
        
        // Mostrar/ocultar beam y varitas
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

        // Interacció si es pitja click esquerre
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentOutlinedObject != null)
        {
            InteractWithCurrentObject();
        }
    }

    private void UpdateBeamPosition()
    {
        // Origen del rayo desde la punta de la varita
        Vector3 rayOrigin = beamOrigin != null ? beamOrigin.position : playerCamera.transform.position;
        
        // Punto focal en el centro de la cámara (donde apunta el jugador)
        Vector3 cameraCenter = playerCamera.transform.position + playerCamera.transform.forward * rayCastDistance;
        Vector3 rayDirection = (cameraCenter - rayOrigin).normalized;

        RaycastHit hit;
        int raycastLayer = LayerMask.NameToLayer(raycastLayerName);
        int outlineLayer = LayerMask.NameToLayer(outlineLayerName);
        int combinedMask = (1 << raycastLayer) | (1 << outlineLayer);

        bool hasHit = Physics.Raycast(rayOrigin, rayDirection, out hit, rayCastDistance, combinedMask);
        float distance = hasHit ? hit.distance : rayCastDistance;

        // Posicionar el beam
        PositionBeam(rayOrigin, rayDirection, distance);

        // Cambiar varita según si hay hit o no
        UpdateWandVisuals(hasHit);

        // Manejar outline
        if (hasHit)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject != currentOutlinedObject)
            {
                DisableCurrentOutline();
                EnableOutline(hitObject);
            }
            SetBeamColor(hitColor);
        }
        else
        {
            DisableCurrentOutline();
            SetBeamColor(missColor);
        }
    }

    private void PositionBeam(Vector3 start, Vector3 direction, float distance)
    {
        // Posicionar el beam para que parta desde BeamOrigin y llegue hasta rayCastDistance
        // El cilindro se posiciona en su centro, así que lo movemos la mitad de la distancia
        Vector3 beamPosition = start + direction * (distance * 0.5f);

        beamVisual.transform.position = beamPosition;
        beamVisual.transform.rotation = Quaternion.LookRotation(direction);
        beamVisual.transform.Rotate(90f, 0f, 0f); // Rotar cilindre perque roti correctament

        // Escalar cilindro: Y = longitud exacta del raycast, X y Z = grosor
        Vector3 newScale = originalBeamScale;
        newScale.y = (distance / 2f) * originalBeamScale.y * beamLengthMultiplier; // Longitud
        newScale.x = originalBeamScale.x * beamThickness; // Grosor
        newScale.z = originalBeamScale.z * beamThickness; // Grosor
        beamVisual.transform.localScale = newScale;

        Debug.Log($"Beam desde BeamOrigin hasta rayCastDistance. Distancia: {distance}, Escala: {newScale}");
    }

    private void SetBeamColor(Color baseColor)
    {
        Color finalColor = new Color(baseColor.r, baseColor.g, baseColor.b, beamOpacity);
        if (beamMaterial != null)
        {
            beamMaterial.color = finalColor;
        }
    }

    private void UpdateWandVisuals(bool hasHit)
    {
        // Forzar actualización si las varitas estaban ocultas o si el estado cambió
        if (wandsVisible && hasHit == lastHitState)
            return;

        lastHitState = hasHit;
        wandsVisible = true;

        if (hasHit)
        {
            //Mostrar varita verde (hit)
            if (greenWand != null) greenWand.SetActive(true);
            if (blueWand != null) blueWand.SetActive(false);
            Debug.Log("Varita verde activada (HIT)");
        }
        else
        {
            //Mostrar varita azul (miss)
            if (blueWand != null) blueWand.SetActive(true);
            if (greenWand != null) greenWand.SetActive(false);
            Debug.Log("Varita azul activada (MISS)");
        }
    }

    private void HideAllWands()
    {
        if (blueWand != null) blueWand.SetActive(false);
        if (greenWand != null) greenWand.SetActive(false);
        wandsVisible = false; // Marcar que las varitas no están visibles
    }

    private void EnableOutline(GameObject targetObject)
    {
        currentOutlinedObject = targetObject;
        originalLayer = targetObject.layer;
        targetObject.layer = LayerMask.NameToLayer(outlineLayerName);
        Debug.Log($"Outline activado en: {targetObject.name}");
    }

    private void DisableCurrentOutline()
    {
        if (currentOutlinedObject != null)
        {
            currentOutlinedObject.layer = originalLayer;
            currentOutlinedObject = null;
        }
    }
    ///Interactua amb l'objecte actual al qual esta apuntant el raycast
    private void InteractWithCurrentObject()
    {
        if (currentOutlinedObject != null)
        {
            GravityController gravityObj = currentOutlinedObject.GetComponent<GravityController>();
            if (gravityObj != null)
            {
                gravityObj.ToggleGravity();
                Debug.Log($"Gravetat canviada en: {currentOutlinedObject.name}. Invertida: {gravityObj.IsGravityInverted()}");
            }
        }
    }

    private void OnDestroy()
    {
        if (beamMaterial != null)
        {
            Destroy(beamMaterial);
        }
    }
}