using UnityEngine;
using System.Collections;

public class MazePuzzleButton : MonoBehaviour
{
    [Header("Referencias")]
    public TripleMazeButtonManager manager;
    public Camera playerCamera;
    public GameObject visualIndicator; 
    public Animator animator; 
    public Transform buttonMesh;

    [Header("Configuración de Interacción")]
    public float interactDistance = 4.0f;
    public KeyCode interactKey = KeyCode.Mouse0;

    [Header("Efecto Visual")]
    public Vector3 pressedOffset = new Vector3(0.1f, 0, 0); 
    public float pressDuration = 0.2f;
    public string boolParameterName = "isHidden"; 

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private bool isActivated = false;
    private bool isProcessing = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (visualIndicator != null) visualIndicator.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && !isActivated && !isProcessing)
        {
            if (IsPlayerPointingAtMe())
            {
                StartCoroutine(ProcessActivation());
            }
        }
    }

    private bool IsPlayerPointingAtMe()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Comprobamos si el impacto es en este objeto o sus hijos
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator ProcessActivation()
    {
        isProcessing = true;
        isActivated = true;

        // 1. Sonido y Feedback Visual
        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        if (visualIndicator != null) visualIndicator.SetActive(true);

        // 2. Movimiento físico (Hundimiento)
        Vector3 originalPos = buttonMesh != null ? buttonMesh.localPosition : Vector3.zero;
        if (buttonMesh != null) buttonMesh.localPosition += pressedOffset;

        // 3. Notificar al Manager
        if (manager != null) manager.NotifyButtonActivated();

        yield return new WaitForSeconds(pressDuration);

        // 4. Retorno físico
        if (buttonMesh != null) buttonMesh.localPosition = originalPos;

        // 5. Animación de "Esconderse"
        if (animator != null)
        {
            animator.SetBool(boolParameterName, true);
        }

        isProcessing = false;
    }
}