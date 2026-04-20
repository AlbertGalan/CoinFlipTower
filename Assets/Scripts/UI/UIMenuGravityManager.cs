using UnityEngine;

public class UIMenuGravityManager : MonoBehaviour
{
    [Header("Gravity Toggle")]
    public KeyCode toggleKey = KeyCode.Space;
    public bool invertAllElements = true;
    
    [Header("Canvas Rotation")]
    public bool rotateCanvas = true;
    public RectTransform canvasToRotate;
    public string containerChildName = "MenuContent";
    public float rotationSpeed = 5f;

    [Header("Player Sync")]
    [Tooltip("Sincronizar automáticamente el menú con la gravedad del jugador")]
    public bool syncWithPlayerGravity = true;
    public GravityController playerGravityController;
    
    [Header("Restrictions")]
    [Tooltip("Bloquear toggle de gravedad durante pausa")]
    public bool blockDuringPause = true;
    [Tooltip("Bloquear toggle durante pausa artificial (tutorial, challenges)")]
    public bool blockDuringArtificialPause = true;
    [Tooltip("Bloquear toggle mientras el jugador empuja o tira de un objeto")]
    public bool blockWhileGrabbing = true;
    
    private bool gravityInverted = false;
    private float targetRotation = 0f;
    private PauseManager pauseManager;
    private Score scoreManager;
    private PushPullController pushPullController;

    private void Awake()
    {
        if (canvasToRotate == null)
        {
            // Buscar hijo por nombre
            Transform child = transform.Find(containerChildName);
            if (child != null)
            {
                canvasToRotate = child.GetComponent<RectTransform>();
                Debug.Log($"Contenedor '{containerChildName}' encontrado y asignado automáticamente");
            }
            
            // Si no se encuentra, usar el propio objeto (pero advertir)
            if (canvasToRotate == null)
            {
                canvasToRotate = GetComponent<RectTransform>();
                Debug.LogWarning($"No se encontró hijo '{containerChildName}'. Usando el objeto raíz. Para Screen Space - Overlay, crea un contenedor hijo.");
            }
        }
        
        // Buscar PauseManager y Score en la escena
        pauseManager = FindFirstObjectByType<PauseManager>();
        scoreManager = FindFirstObjectByType<Score>();
        pushPullController = FindFirstObjectByType<PushPullController>();

        if (playerGravityController == null)
        {
            playerGravityController = FindPlayerGravityController();
        }

        if (syncWithPlayerGravity && playerGravityController != null)
        {
            SetGravityInverted(playerGravityController.IsGravityInverted());
        }
    }

    private void Update()
    {
        if (syncWithPlayerGravity)
        {
            if (playerGravityController == null)
            {
                playerGravityController = FindPlayerGravityController();
            }

            if (playerGravityController != null)
            {
                bool playerInverted = playerGravityController.IsGravityInverted();
                if (playerInverted != gravityInverted)
                {
                    SetGravityInverted(playerInverted);
                }
            }
        }

        // Detectar tecla de cambio de gravedad
        if (!syncWithPlayerGravity && Input.GetKeyDown(toggleKey))
        {
            // Verificar restricciones
            if (IsGravityToggleBlocked())
            {
                return;
            }
            
            ToggleGravity();
        }

        // Rotar canvas suavemente
        if (rotateCanvas && canvasToRotate != null)
        {
            Vector3 currentRotation = canvasToRotate.localEulerAngles;
            float currentZ = currentRotation.z;
            
            // Normalizar ángulo
            if (currentZ > 180f) currentZ -= 360f;
            
            float newZ = Mathf.LerpAngle(currentZ, targetRotation, Time.unscaledDeltaTime * rotationSpeed);
            canvasToRotate.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, newZ);
        }
    }

    private bool IsGravityToggleBlocked()
    {
        // Bloquear si el juego está pausado
        if (blockDuringPause && pauseManager != null && pauseManager.IsPaused)
        {
            return true;
        }
        
        // Bloquear si hay pausa artificial (tutorial, challenges)
        if (blockDuringArtificialPause && scoreManager != null && scoreManager.IsGameplayFrozen)
        {
            return true;
        }

        // Bloquear si el jugador esta empujando o tirando de un objeto
        if (blockWhileGrabbing && pushPullController != null && pushPullController.IsGrabbing)
        {
            return true;
        }
        
        return false;
    }

    private void ToggleGravity()
    {
        gravityInverted = !gravityInverted;
        targetRotation = gravityInverted ? 180f : 0f;

        // Invertir gravedad de todos los elementos UIMenuGravity
        if (invertAllElements)
        {
            UIMenuGravity[] gravityElements = FindObjectsByType<UIMenuGravity>(FindObjectsSortMode.None);
            foreach (UIMenuGravity element in gravityElements)
            {
                element.SetGravityInverted(gravityInverted);
            }
        }
    }

    public void SetGravityInverted(bool inverted)
    {
        gravityInverted = inverted;
        targetRotation = gravityInverted ? 180f : 0f;

        if (invertAllElements)
        {
            UIMenuGravity[] gravityElements = FindObjectsByType<UIMenuGravity>(FindObjectsSortMode.None);
            foreach (UIMenuGravity element in gravityElements)
            {
                element.SetGravityInverted(gravityInverted);
            }
        }
    }

    public bool IsGravityInverted()
    {
        return gravityInverted;
    }

    private GravityController FindPlayerGravityController()
    {
        GravityController[] controllers = FindObjectsByType<GravityController>(FindObjectsSortMode.None);
        foreach (GravityController controller in controllers)
        {
            if (controller != null && controller.isPlayer)
            {
                return controller;
            }
        }

        return null;
    }
}
