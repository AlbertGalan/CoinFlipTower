using UnityEngine;

public class FlappyBallLogic : MonoBehaviour
{
    public float moveSpeed = 8f;
    private Rigidbody rb;
    private GravityController gravityScript;
    private FlappyPuzzleManager manager;

    [HideInInspector] public bool canMove = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityScript = GetComponent<GravityController>();
        manager = FindFirstObjectByType<FlappyPuzzleManager>();
        
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (!canMove) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (gravityScript != null)
            {
                gravityScript.ToggleGravity();
            }
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        if (rb.isKinematic) rb.isKinematic = false;

        // h * -1 es la inversión general para los niveles 1 y 2
        float h = Input.GetAxis("Horizontal") * -1f; 

        // AJUSTE NIVEL 3: Si es el tercer nivel, invertimos el valor de h
        if (manager.activeCheckpointIndex == 2)
        {
            h *= -1f; 
        }

        Vector3 moveDirection = Vector3.zero;

        // Lógica de movimiento según el nivel (Eje X o Eje Z)
        if (manager.activeCheckpointIndex == 0 || manager.activeCheckpointIndex == 2)
        {
            // Nivel 1 y 3 se mueven en el eje X
            moveDirection = new Vector3(h, 0, 0);
        }
        else if (manager.activeCheckpointIndex == 1)
        {
            // Nivel 2 se mueve en el eje Z
            moveDirection = new Vector3(0, 0, h);
        }

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Vector3 targetMove = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + targetMove);
        }
    }

  // Dentro de FlappyBallLogic.cs en el OnTriggerEnter
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Lava"))
    {
        manager.RespawnBall();
    }
    
    // Si le pones el tag "Finish" al objeto de la meta:
    if (other.CompareTag("Meta"))
    {
        this.canMove = false; // Desactiva Update y FixedUpdate
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // La dejamos clavada en la meta
    }
}
}