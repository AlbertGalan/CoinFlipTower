using UnityEngine;
using System.Collections;

public class StroopLever : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;
    public MoveCharacter moveCharacter;
    public Transform leverVisual;
    public StroopPuzzleManager stroopManager; // Referencia al manager del puzzle

    [Header("Ajustes de Interacción")]
    public float interactionDistance = 3f;
    public float pulledAngleX = 30f;
    public float restAngleX = -30f;
    public float pullSensitivity = 6f;
    public float returnSpeed = 90f;

    private float currentAngleX;
    private bool isHolding;
    private bool triggered;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        currentAngleX = restAngleX;
    }

    void Update()
    {
        // Si el puzzle ya ha sido superado (5+ aciertos), la palanca se queda bloqueada
        if (stroopManager != null && stroopManager.IsPuzzleFullySolved) 
        {
            currentAngleX = pulledAngleX;
            leverVisual.localRotation = Quaternion.Euler(currentAngleX, 0, 0);
            return;
        }

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
                // Solo podemos tirar si el manager no está ya en una partida
                if (stroopManager != null && !stroopManager.IsGameActive)
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
            if (stroopManager != null)
            {
                stroopManager.StartPuzzle();
            }
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
        // La palanca vuelve a su sitio solo si el puzzle no está activo
        // Si el puzzle está activo, se queda abajo hasta que termine
        float target = (stroopManager != null && stroopManager.IsGameActive) ? pulledAngleX : restAngleX;
        
        currentAngleX = Mathf.MoveTowards(currentAngleX, target, returnSpeed * Time.deltaTime);
        leverVisual.localRotation = Quaternion.Euler(currentAngleX, 0, 0);
    }
}