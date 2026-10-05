using UnityEngine;
using System.Collections;

public class PressureCheckButton : MonoBehaviour
{
    [Header("Referencias")]
    public PressurePuzzleManager manager;
    [Tooltip("Si se deja vacío, buscará la cámara principal automáticamente")]
    public Camera playerCamera;
    
    [Header("Configuración de Interacción")]
    public float interactDistance = 4.0f;
    public KeyCode interactKey = KeyCode.Mouse0; // Clic Izquierdo

    [Header("Animación Visual")]
    public Transform buttonMesh; 
    public Vector3 pressedOffset = new Vector3(0, -0.1f, 0);
    public float pressDuration = 0.2f;

    private bool isProcessing = false;

    
      [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    void Start()
    {
        // Si no asignaste cámara en el inspector, buscamos la MainCamera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        // Detectamos el clic
        if (Input.GetKeyDown(interactKey) && !isProcessing)
        {
            if (IsPlayerPointingAtMe())
            {
                Interact();
            }
        }
    }

    private bool IsPlayerPointingAtMe()
    {
        if (playerCamera == null) return false;

        // Lanzamos el rayo exactamente desde el centro de la visión del jugador
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Dibujamos el rayo en la ventana de Scene para que veas si llegas (solo en Editor)
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.magenta);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Verificamos si lo que ha golpeado el rayo es este botón o parte de él
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        return false;
    }

    public void Interact()
    {
        if (!isProcessing)
        {
            StartCoroutine(ProcessActivation());
        }
    }

    private IEnumerator ProcessActivation()
    {

        isProcessing = true;

   // 1. Sonido y Feedback Visual
        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        // 1. Animación física
        Vector3 originalPos = buttonMesh != null ? buttonMesh.localPosition : Vector3.zero;
        if (buttonMesh != null) buttonMesh.localPosition += pressedOffset;

        // 2. Notificar al Manager
        if (manager != null)
        {
            manager.TryActivate();
        }

        yield return new WaitForSeconds(pressDuration);

        // 3. Retorno del botón
        if (buttonMesh != null) buttonMesh.localPosition = originalPos;
        
        // Cooldown para evitar doble clic accidental
        yield return new WaitForSeconds(0.3f);
        isProcessing = false;
    }
}