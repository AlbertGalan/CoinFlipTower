using UnityEngine;
using TMPro;

public class FlappyPuzzleManager : MonoBehaviour
{
    [Header("Referencias")]
    public MoveCharacter playerScript;
    public GameObject ballObject;
    public FlappyBallLogic ballLogic;
    public Camera flappyCamera; 
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI promptText;

    [Header("Checkpoints")]
    public Transform[] checkpoints; 
    public int activeCheckpointIndex = 0;

    [Header("Configuración Cámaras")]
    [Tooltip("Arrastra aquí los 3 GameObjects vacíos que sirven como posiciones de cámara")]
    public Transform[] cameraPoints;

    private bool isPlayerNearby = false;
    private bool isPlaying = false;
    private bool puzzleSolved = false;
    void Start()
    {
        ballObject.SetActive(false);
        flappyCamera.gameObject.SetActive(false);
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if(!puzzleSolved) promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            promptText.gameObject.SetActive(false);
        }
    }

   void Update()
{
    // Si el puzzle está resuelto, solo permitimos pulsar E para salir si aún estamos jugando (en vista de bola)
    if (puzzleSolved)
    {
        if (isPlaying && Input.GetKeyDown(KeyCode.E))
        {
            ToggleMode(); // Esto devolverá la cámara al personaje
        }
        return;
    }

    if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
    {
        ToggleMode();
    }
}

    void ToggleMode()
    {
        isPlaying = !isPlaying;
        GravityController playerGravity = playerScript.GetComponent<GravityController>();

        if (isPlaying)
        {
            playerScript.SetBlockMovement(true);
            playerScript.SetLockCamera(true);
            if (playerGravity != null) playerGravity.isPlayer = false;

            ballObject.SetActive(true);
            
            Rigidbody ballRb = ballObject.GetComponent<Rigidbody>();
            if(ballRb != null) ballRb.isKinematic = false;

            ballLogic.canMove = true;
            flappyCamera.gameObject.SetActive(true);
            
            // Al entrar, forzamos la posición de cámara inicial
            UpdateCameraPosition();
            RespawnBall();
        }
        else
        {
            playerScript.SetBlockMovement(false);
            playerScript.SetLockCamera(false);
            if (playerGravity != null) playerGravity.isPlayer = true;

            ballLogic.canMove = false;
            flappyCamera.gameObject.SetActive(false);
            ballObject.SetActive(false);
        }
    }

    public void SetCheckpoint(int index)
    {
        if (index > activeCheckpointIndex)
        {
            activeCheckpointIndex = index;
            UpdateCameraPosition();
            UpdateUI();
        }
    }

    void UpdateCameraPosition()
    {
        // Verificamos que el índice exista en nuestro array de Transforms de cámara
        if (activeCheckpointIndex < cameraPoints.Length && cameraPoints[activeCheckpointIndex] != null)
        {
            Transform targetCam = cameraPoints[activeCheckpointIndex];
            flappyCamera.transform.position = targetCam.position;
            flappyCamera.transform.rotation = targetCam.rotation;
        }
    }

    public void RespawnBall()
    {
        ballObject.transform.position = checkpoints[activeCheckpointIndex].position;
        Rigidbody rb = ballObject.GetComponent<Rigidbody>();
        
        rb.isKinematic = false; 
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        ballObject.GetComponent<GravityController>().SetGravityInverted(false);
    }

    void UpdateUI()
    {
        levelText.text = $"Nivell {activeCheckpointIndex + 1}/3";
    }

   public void CompletePuzzle()
{
    puzzleSolved = true;
    // IMPORTANTE: Ya NO llamamos a ToggleMode() aquí automáticamente.
    // Solo actualizamos la UI para avisar al jugador.
    
    levelText.text = "Completat";
    
    // Cambiamos el prompt para que el jugador sepa cómo salir
    promptText.text = "Prem E per sortir";
    promptText.gameObject.SetActive(true);
}
}