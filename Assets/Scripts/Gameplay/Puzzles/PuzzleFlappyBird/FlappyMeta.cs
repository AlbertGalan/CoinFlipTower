using UnityEngine;
using UnityEngine.Events;

public class FlappyMeta : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El tag que debe tener la bola para activar el evento")]
    public string targetTag = "Ball"; // O el tag que uses para la bola

    [Header("Evento")]
    public UnityEvent onTriggerActivated;

    private bool activated = false;

private void OnTriggerEnter(Collider other)
{
    if (activated) return;

    if (other.CompareTag(targetTag))
    {
        activated = true;
        
        // Ejecutamos el mecanismo (puertas, luces, etc.)
        onTriggerActivated?.Invoke();

        // Avisamos al manager de que hemos llegado
        FlappyPuzzleManager manager = FindFirstObjectByType<FlappyPuzzleManager>();
        if (manager != null)
        {
            manager.CompletePuzzle(); 
        }
    }
}
}