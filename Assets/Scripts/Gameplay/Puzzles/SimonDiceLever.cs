using UnityEngine;

public class SimonDiceLever : MonoBehaviour
{
    [Header("References")]
    public SimonDiceManager simonManager;
    public Camera playerCamera;
    public MoveCharacter moveCharacter; // Tu script de personaje
    public Transform leverVisual;

    [Header("Interaction Settings")]
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
                if (moveCharacter != null) {
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

        if (!triggered && Mathf.Abs(currentAngleX - pulledAngleX) < 5f)
        {
            triggered = true;
            simonManager.IniciarJuego();
        }
    }

    private void ReleaseLever()
    {
        isHolding = false;
        if (moveCharacter != null) {
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