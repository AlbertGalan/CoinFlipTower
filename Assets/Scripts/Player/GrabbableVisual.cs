using UnityEngine;

/// <summary>
/// Visual outline for grabbable objects using layer-based system.
/// Switches object to "Outline" layer when highlighted for proximity feedback.
/// Attach to any object you want to show outline when the player is near.
/// </summary>
public class GrabbableVisual : MonoBehaviour
{
    [Header("Layer Settings")]
    [Tooltip("Name of the outline layer to switch to when highlighted (use a dedicated layer like 'OutlineGrabbable' for yellow outline)")]
    [SerializeField] private string outlineLayerName = "OutlineGrabbable";
    private int originalLayer;
    private bool isHighlighted = false;

    void Awake()
    {
        // Guardar la layer original del objeto
        originalLayer = gameObject.layer;
    }

    public void Highlight(bool on)
    {
        if (on && !isHighlighted)
        {
            // Activar outline cambiando a la layer de Outline
            gameObject.layer = LayerMask.NameToLayer(outlineLayerName);
            isHighlighted = true;
        }
        else if (!on && isHighlighted)
        {
            // Desactivar outline restaurando la layer original
            gameObject.layer = originalLayer;
            isHighlighted = false;
        }
    }

    private void OnDestroy()
    {
        // Restaurar layer original al destruir el objeto
        if (isHighlighted)
        {
            gameObject.layer = originalLayer;
        }
    }
}
