using UnityEngine;

public class RayCast : MonoBehaviour
{
    [Header("Raycast settings")]
    [SerializeField] private float rayCastDistance = 4.0f;
    [SerializeField] private LayerMask raycastLayerMask;

    [Header("Layer names")]
    [SerializeField] private string raycastLayerName = "Raycast";
    [SerializeField] private string outlineLayerName = "Outline";

    [Header("Input")]
    [SerializeField] private KeyCode outlineKey = KeyCode.Mouse1;

    [Header("Beam Settings")]
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.cyan;
    [SerializeField] [Range(0f, 1f)] private float beamOpacity = 1f; // Poner a 1 temporalmente
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject beamVisual;
    
    private GameObject currentOutlinedObject;
    private int originalLayer;
    private Material beamMaterial;
    private Vector3 originalBeamScale;

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
        HandleOutline();
    }

    private void HandleOutline()
    {
        bool shouldShowBeam = Input.GetKey(outlineKey);
        
        if (beamVisual.activeSelf != shouldShowBeam)
        {
            beamVisual.SetActive(shouldShowBeam);
            
            if (!shouldShowBeam)
            {
                DisableCurrentOutline();
                return;
            }
        }

        if (!shouldShowBeam) return;

        UpdateBeamPosition();
    }

    private void UpdateBeamPosition()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        RaycastHit hit;
        int raycastLayer = LayerMask.NameToLayer(raycastLayerName);
        int outlineLayer = LayerMask.NameToLayer(outlineLayerName);
        int combinedMask = (1 << raycastLayer) | (1 << outlineLayer);

        bool hasHit = Physics.Raycast(rayOrigin, rayDirection, out hit, rayCastDistance, combinedMask);
        float distance = hasHit ? hit.distance : rayCastDistance;

        // Posicionar el beam
        PositionBeam(rayOrigin, rayDirection, distance);

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
        // Posicionar en el punto medio entre start y end
        Vector3 endPoint = start + direction * distance;
        Vector3 midPoint = start + direction * (distance * 0.5f);

        beamVisual.transform.position = midPoint;
        beamVisual.transform.rotation = Quaternion.LookRotation(direction);
        beamVisual.transform.Rotate(90f, 0f, 0f); // Rotar cilindre perque roti correctament

        // Escalar cilindre en base a la distància
        Vector3 newScale = originalBeamScale;
        newScale.y = (distance / 2f) * originalBeamScale.y;
        beamVisual.transform.localScale = newScale;

        Debug.Log($"Beam posicionado. Distancia: {distance}, Escala: {newScale}");
    }

    private void SetBeamColor(Color baseColor)
    {
        Color finalColor = new Color(baseColor.r, baseColor.g, baseColor.b, beamOpacity);
        if (beamMaterial != null)
        {
            beamMaterial.color = finalColor;
        }
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

    private void OnDestroy()
    {
        if (beamMaterial != null)
        {
            Destroy(beamMaterial);
        }
    }
}