using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IceBlockSlider : MonoBehaviour
{
    [Header("Ice Sliding Settings")]
    public float slideSpeedMultiplier = 1.35f;
    public float obstacleCheckRadius = 0.4f;
    [Tooltip("Cuanta inercia mantiene en el aire (1.0 = no frena nada)")]
    public float airMomentumPreservation = 0.99f;

    private bool isSliding = false;
    private Vector3 slideDirection = Vector3.zero;
    private float baseSpeed = 10f;
    private Rigidbody rb;
    private GravityController gravityCtrl;
    private Vector3 lateralMomentum = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityCtrl = GetComponent<GravityController>();
        
        // Buscamos la velocidad del jugador para que el bloque se mueva acorde al mundo
        MoveCharacter mc = FindFirstObjectByType<MoveCharacter>();
        if (mc != null) baseSpeed = mc.moveSpeed;
    }

    // Este es el método que llama el Spawner
    public void StartSliding(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            isSliding = true;
            slideDirection = direction.normalized;

            // --- MODIFICACIÓN CLAVE ---
            // Calculamos el momentum inicial aquí. Si no, en el aire lateralMomentum es 0
            // y el bloque se detiene antes de tocar el suelo.
            lateralMomentum = slideDirection * (baseSpeed * slideSpeedMultiplier);

            // Limpiamos la velocidad física para que MovePosition tome el control suavemente
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
{
    if (!isSliding) return;

    // 1. Detección de suelo/hielo RELATIVA A LA GRAVEDAD
    bool overIce = false;
    bool isGrounded = false;
    RaycastHit hitDown;
    
    // USAMOS LA DIRECCIÓN DE LA GRAVEDAD REAL
    // Si gravityInverted es true, la gravedad va hacia Vector3.up, 
    // así que el rayo debe ir hacia Vector3.up para "pisar" el techo.
    Vector3 gravityDir = gravityCtrl.IsGravityInverted() ? Vector3.up : Vector3.down;
    
    // El origen lo movemos un poco EN CONTRA de la gravedad para que empiece dentro del bloque
    Vector3 rayOrigin = rb.position - gravityDir * 0.25f;

    // Disparamos el rayo en la misma dirección que la gravedad
    if (Physics.Raycast(rayOrigin, gravityDir, out hitDown, 1.2f, Physics.AllLayers, QueryTriggerInteraction.Collide))
    {
        isGrounded = true;
        if (hitDown.collider.CompareTag("Ice"))
        {
            overIce = true;
        }
    }

        // 2. Lógica de estados
        if (isGrounded)
        {
            if (overIce)
            {
                ManejarMovimientoHielo();
            }
            else
            {
                // Si toca suelo normal (no hielo), se frena en seco
                StopSliding();
            }
        }
        else
        {
            // Si está en el aire (vuelo tras disparo o cambio de gravedad)
            ManejarVueloInercia();
        }
    }

    private void ManejarMovimientoHielo()
    {
        float moveDistance = (baseSpeed * slideSpeedMultiplier) * Time.fixedDeltaTime;
        
        if (ComprobarObstaculos(slideDirection, moveDistance))
        {
            StopSliding();
        }
        else
        {
            Vector3 newPosition = rb.position + slideDirection * moveDistance;
            rb.MovePosition(newPosition);
            
            // Actualizamos momentum mientras resbala
            lateralMomentum = slideDirection * (baseSpeed * slideSpeedMultiplier);
        }
    }

    private void ManejarVueloInercia()
    {
        // Usamos el momentum que ya tenemos (ahora no será 0 al inicio)
        float moveDistance = lateralMomentum.magnitude * Time.fixedDeltaTime;
        Vector3 flyDirection = lateralMomentum.normalized;

        if (moveDistance > 0.0001f)
        {
            if (ComprobarObstaculos(flyDirection, moveDistance))
            {
                StopSliding();
            }
            else
            {
                Vector3 newPosition = rb.position + flyDirection * moveDistance;
                rb.MovePosition(newPosition);
                
                // Aplicamos fricción de aire
                lateralMomentum *= airMomentumPreservation;
            }
        }

        // Si el bloque se queda casi parado en el aire, dejamos de procesar el slide
        if (lateralMomentum.sqrMagnitude < 0.1f) 
        {
            StopSliding();
        }
    }

    private bool ComprobarObstaculos(Vector3 direction, float distance)
    {
        // El origen del SphereCast debe estar en el centro del bloque
        Vector3 sphereOrigin = rb.position + transform.up * 0.5f;
        RaycastHit hitWall;

        if (Physics.SphereCast(sphereOrigin, obstacleCheckRadius, direction, out hitWall, distance + 0.1f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            // No chocar con triggers ni con uno mismo
            if (hitWall.collider != GetComponent<Collider>() && !hitWall.collider.isTrigger)
            {
                return true; 
            }
        }
        return false;
    }

    private void StopSliding()
    {
        isSliding = false;
        slideDirection = Vector3.zero;
        lateralMomentum = Vector3.zero;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}