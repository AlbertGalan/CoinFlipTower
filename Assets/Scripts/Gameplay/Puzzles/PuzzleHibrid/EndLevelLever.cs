using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EndLevelLever : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;
    public Camera cinematicCamera; // La cámara que muestra la sala
    public MoveCharacter moveCharacter;
    public Transform leverVisual;
    public GameObject floorLava;
    public GameObject roofLava;
    public GameObject portalToPreviousRoom;
    public UnityEvent onMechanismActivated; // Para abrir la puerta de salida

    [Header("Ajustes de Interacción")]
    public float interactionDistance = 3f;
    public float pulledAngleX = 30f;
    public float restAngleX = -30f;
    public float pullSensitivity = 6f;
    public float returnSpeed = 90f;

    [Header("Cinemática")]
    public float cinematicDuration = 5f;
    public Vector3 targetRoofLavaPos; // Posición final de la lava (escondida en el techo)

    private float currentAngleX;
    private bool isHolding;
    private bool triggered;
    private bool sequenceFinished = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (portalToPreviousRoom != null) portalToPreviousRoom.SetActive(false);
        currentAngleX = restAngleX;
    }

    void Update()
    {
        if (sequenceFinished) return;

        if (Input.GetMouseButtonDown(0)) TryBeginHold();

        if (isHolding)
        {
            if (Input.GetMouseButton(0)) UpdatePull();
            if (Input.GetMouseButtonUp(0)) ReleaseLever();
        }
        else
        {
            ReturnToRest();
        }
    }

    private void TryBeginHold()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.transform == leverVisual || hit.transform.IsChildOf(leverVisual))
            {
                isHolding = true;
                triggered = false;
                if (moveCharacter != null)
                {
                    moveCharacter.SetLockCamera(true);
                    moveCharacter.SetBlockMovement(true);
                }
            }
        }
    }

    private void UpdatePull()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        currentAngleX = Mathf.Clamp(currentAngleX - mouseY * pullSensitivity, Mathf.Min(restAngleX, pulledAngleX), Mathf.Max(restAngleX, pulledAngleX));
        leverVisual.localRotation = Quaternion.Euler(currentAngleX, 0, 0);

        // Si tiramos de la palanca lo suficiente
        if (!triggered && Mathf.Abs(currentAngleX - pulledAngleX) < 5f)
        {
            triggered = true;
            sequenceFinished = true; // Desactivamos el Update de la palanca
            StartCoroutine(CinematicSequence());
        }
    }

    private IEnumerator CinematicSequence()
    {
        // 1. Cambio de cámara
        playerCamera.gameObject.SetActive(false);
        cinematicCamera.gameObject.SetActive(true);

        // 2. Activar mecanismos (Puerta salida)
        onMechanismActivated.Invoke();

        // 3. Animación visual de la lava subiendo al techo
        float elapsed = 0;
        Vector3 startPos = roofLava.transform.position;
        // Destruimos el script de "LavaBreathing" si lo tiene para que no interfiera
        LavaBreathing lb = roofLava.GetComponentInParent<LavaBreathing>();
        if (lb != null) lb.enabled = false;

        while (elapsed < cinematicDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cinematicDuration;
            // Subimos la lava suavemente
            if (roofLava != null)
                roofLava.transform.position = Vector3.Lerp(startPos, targetRoofLavaPos, t);
            
            // Opcional: Bajar/Desaparecer la del suelo
            if (floorLava != null)
                floorLava.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

            yield return null;
        }

        // 4. Cambio de cámara de vuelta al jugador
        playerCamera.gameObject.SetActive(true);
        Destroy(cinematicCamera.gameObject);

        // 5. Destrucción de objetos de lava
        Destroy(floorLava);
        Destroy(roofLava);

        // 6. Aparece el portal
        if (portalToPreviousRoom != null)
        {
            portalToPreviousRoom.SetActive(true);
        }

        // 7. Liberar al jugador
        if (moveCharacter != null)
        {
            moveCharacter.SetLockCamera(false);
            moveCharacter.SetBlockMovement(false);
        }
    }

    private void ReleaseLever()
    {
        isHolding = false;
        if (moveCharacter != null)
        {
            moveCharacter.SetLockCamera(false);
            moveCharacter.SetBlockMovement(false);
        }
    }

    private void ReturnToRest()
    {
        currentAngleX = Mathf.MoveTowards(currentAngleX, restAngleX, returnSpeed * Time.deltaTime);
        leverVisual.localRotation = Quaternion.Euler(currentAngleX, 0, 0);
    }
}