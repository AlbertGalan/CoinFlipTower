using UnityEngine;
using UnityEngine.Events;

public class EasterDS : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private GameObject objectToActivate; // El objeto que aparecerá

    [Header("Audio Settings")]
    [SerializeField] private AudioClip easterEggSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    [Header("Events")]
    public UnityEvent onActivateEasterEgg;

    private bool alreadyActivated = false;

    public void ActivateEasterDS()
    {
        if (alreadyActivated) return;
        alreadyActivated = true;

        // 1. Activar el objeto visual
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        // 2. Sonar el audio sin necesidad de AudioSource (se crea en la posición del objeto)
        if (easterEggSound != null)
        {
            AudioSource.PlayClipAtPoint(easterEggSound, transform.position, volume);
        }

        // 3. Disparar el Unity Event
        onActivateEasterEgg?.Invoke();

        Debug.Log($"<color=yellow>EasterDS:</color> {gameObject.name} activado!");
    }
}