using UnityEngine;

public class WeightBlock : MonoBehaviour
{
    [Header("Block Weight Settings")]
    [Tooltip("El valor numérico que aporta este bloque a la balanza")]
    public int weightValue = 1;

    [Header("Visual Feedback (Opcional)")]
    [Tooltip("Mostrar el número sobre el bloque en la escena")]
    public bool showValueInScene = true;

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showValueInScene)
        {
            // Muestra el valor del bloque en la vista de Scene de Unity
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                "Peso: " + weightValue.ToString(),
                new GUIStyle()
                {
                    fontSize = 14,
                    normal = new GUIStyleState() { textColor = Color.yellow }
                });
        }
    }
    #endif

    public int GetWeight()
    {
        return weightValue;
    }
}
