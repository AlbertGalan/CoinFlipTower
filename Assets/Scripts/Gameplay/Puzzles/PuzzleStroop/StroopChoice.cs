using UnityEngine;
using TMPro;

public class StroopChoice : MonoBehaviour
{
    [HideInInspector] public string colorName; // Lo asigna el Manager ahora
    public TextMeshProUGUI textUI; // Arrastra el texto del canvas de la casilla aquí
    
    [Header("Materiales Feedback")]
    public Material correctMaterial;
    public Material wrongMaterial;
    
    private Renderer cubeRenderer;
    private Material originalMaterial;

    void Awake()
    {
        cubeRenderer = GetComponent<Renderer>();
        // Importante: Usar sharedMaterial para no crear instancias en el Awake
        originalMaterial = cubeRenderer.sharedMaterial;
    }

    public void SetVisualFeedback(bool isCorrect)
    {
        cubeRenderer.material = isCorrect ? correctMaterial : wrongMaterial;
        Invoke(nameof(ResetMaterial), 0.5f);
    }

    private void ResetMaterial()
    {
        cubeRenderer.material = originalMaterial;
    }
}