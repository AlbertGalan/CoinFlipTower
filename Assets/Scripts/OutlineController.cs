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

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LineRenderer lineRenderer; 

    private GameObject currentOutlinedObject;
    private int originalLayer;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        // Crear LineRenderer si no existeix
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCapVertices = 1;
            lineRenderer.numCornerVertices = 1;
        }

        lineRenderer.enabled = false;
        // Aseguramos que tenga 2 posiciones (start, end)
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        HandleOutline();
    }

    private void HandleOutline()
    {
        // Si no es manté pitjat click dret, desactivam outline i línia
        if (!Input.GetKey(outlineKey))
        {
            DisableCurrentOutline();
            if (lineRenderer != null) lineRenderer.enabled = false;
            return;
        }

        // Ray desde el centre de la càmera
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        // Activam la visual del ray
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, ray.origin);
        }

        RaycastHit hit;

        // Crear màscara combinada per detectar Raycast + Outline
        int raycastLayer = LayerMask.NameToLayer(raycastLayerName);
        int outlineLayer = LayerMask.NameToLayer(outlineLayerName);
        int combinedMask = (1 << raycastLayer) | (1 << outlineLayer);

        // Realitzar el raycast amb la màscara combinada
        if (Physics.Raycast(ray, out hit, rayCastDistance, combinedMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Si l'objecte impactat és diferent de l'actual, actualitzem l'outline
            if (hitObject != currentOutlinedObject)
            {
                DisableCurrentOutline();
                EnableOutline(hitObject);
            }

            //Actualitzam la visual del ray
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(1, hit.point);
                SetLineColor(Color.green);
            }
        }
        else
        {
            // No hi ha impacte: desactivar outline
            DisableCurrentOutline();
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(1, ray.origin + ray.direction * rayCastDistance);
                SetLineColor(Color.cyan);
            }
        }
    }

    private void EnableOutline(GameObject targetObject)
    {
        currentOutlinedObject = targetObject;

        originalLayer = targetObject.layer; // Guardamos el layer original
        targetObject.layer = LayerMask.NameToLayer(outlineLayerName);

        Debug.Log($"Outline activado en {targetObject.name}");
    }

    private void DisableCurrentOutline()
    {
        if (currentOutlinedObject != null)
        {
            currentOutlinedObject.layer = originalLayer;
            currentOutlinedObject = null;
        }
    }

    // per canviar el color de la línia del LineRenderer
    private void SetLineColor(Color color)
    {
        if (lineRenderer == null) return;

        if (lineRenderer.material != null)
        {
            lineRenderer.material.color = color;
        }
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}
