using UnityEngine;
using System.Collections;

public class PuzzleButton : MonoBehaviour
{
    [Header("Referencias")]
    public SimpleDoubleButtonManager manager;
    public Camera playerCamera;
    public GameObject visualIndicator; 
    public Animator animator; 
    public Transform buttonMesh;

    [Header("Configuración de Interacción")]
    public float interactDistance = 4.0f;
    public KeyCode interactKey = KeyCode.Mouse0;

    [Header("Efecto de Hundimiento (Físico)")]
    public Vector3 pressedOffset = new Vector3(0.1f, 0, 0); 
    public float pressDuration = 0.2f;

    [Header("Animación (Animator)")]
    public string boolParameterName = "isHidden"; // Cambiado de trigger a bool

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
        if (playerCamera == null) return false;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.magenta);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
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

        // 1. Hundimiento físico ("Click")
        Vector3 originalPos = buttonMesh != null ? buttonMesh.localPosition : Vector3.zero;
        if (buttonMesh != null) buttonMesh.localPosition += pressedOffset;

        if (visualIndicator != null) visualIndicator.SetActive(true);
        if (manager != null) manager.NotifyButtonActivated();

        yield return new WaitForSeconds(pressDuration);

        // 2. Retorno físico antes de esconderse del todo
        if (buttonMesh != null) buttonMesh.localPosition = originalPos;

        // 3. CAMBIO CLAVE: Activamos el booleano en el Animator
        if (animator != null)
        {
            animator.SetBool(boolParameterName, true);
        }

        isProcessing = false;
        Debug.Log(gameObject.name + " activado (Boolean set to true).");
    }
}