using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IceBlockSlider : MonoBehaviour
{
    [Header("Ice Sliding Settings")]
    public float slideSpeedMultiplier = 1.35f;
    public float obstacleCheckRadius = 0.4f;
    [Tooltip("Inercia en el aire (1.0 = no frena nada)")]
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
        MoveCharacter mc = FindFirstObjectByType<MoveCharacter>();
        if (mc != null) baseSpeed = mc.moveSpeed;
    }

    public void StartSliding(Vector3 direction, float speedOverride = -1f)
    {
        // LOG de auditoría para comparar con el jugador
        //Debug.Log($"<color=orange>[ICE EVENT]</color> StartSliding en <b>{gameObject.name}</b>. " + $"Dir: {direction}, Speed: {speedOverride}");

        if (direction.sqrMagnitude > 0.001f)
        {
            isSliding = true;
            slideDirection = direction.normalized;
            
            // Si speedOverride es -1 (como manda el jugador), usamos baseSpeed * multiplier
            float targetSpeed = (speedOverride > 0) ? speedOverride : (baseSpeed * slideSpeedMultiplier);
            lateralMomentum = slideDirection * targetSpeed;

            // Limpiamos fuerzas para que MovePosition tenga control total
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    void FixedUpdate()
    {
        if (!isSliding) return;

        // 1. DETECCIÓN DE SUELO MEJORADA
        Vector3 gravityDir = (gravityCtrl != null && gravityCtrl.IsGravityInverted()) ? Vector3.up : Vector3.down;
        
        // Subimos el origen un poco más para que el rayo no empiece debajo del suelo si el bloque se hunde un poco
        Vector3 rayOrigin = rb.position - (gravityDir * 0.1f); 
        
        RaycastHit hitDown;
        bool isGrounded = false;
        bool overIce = false;

        // Aumentamos a 2.5f para tolerar pequeños saltos o rebotes al nacer del spawner
        if (Physics.Raycast(rayOrigin, gravityDir, out hitDown, 2.5f, Physics.AllLayers, QueryTriggerInteraction.Collide))
        {
            isGrounded = true;
            if (hitDown.collider.CompareTag("Ice")) 
            {
                overIce = true;
            }
        }

        // 2. LÓGICA DE ESTADOS CON DEBUG
        if (isGrounded)
        {
            if (overIce)
            {
                ManejarMovimientoHielo();
            }
            else
            {
                // Si se para, queremos saber por qué
                ///Debug.Log($"<color=red>[IceSlider]</color> {gameObject.name} se detiene: Suelo detectado pero NO es Ice (Tag: {hitDown.collider.tag})");
                StopSliding();
            }
        }
        else
        {
            // Si está en el aire (rebote inicial), mantenemos la inercia para que no se pare en seco
            ManejarVueloInercia();
        }
    }

    private void ManejarMovimientoHielo()
    {
        float moveDistance = lateralMomentum.magnitude * Time.fixedDeltaTime;
        
        if (ComprobarObstaculos(slideDirection, moveDistance))
        {
            //Debug.Log($"<color=yellow>[IceSlider]</color> {gameObject.name} chocó con obstáculo.");
            StopSliding();
        }
        else
        {
            Vector3 newPosition = rb.position + slideDirection * moveDistance;
            rb.MovePosition(newPosition);
        }
    }

    private void ManejarVueloInercia()
    {
        float moveDistance = lateralMomentum.magnitude * Time.fixedDeltaTime;
        if (moveDistance > 0.001f)
        {
            if (!ComprobarObstaculos(slideDirection, moveDistance))
            {
                rb.MovePosition(rb.position + slideDirection * moveDistance);
                // Frenado muy suave en el aire para que no se detenga durante el rebote inicial
                lateralMomentum *= airMomentumPreservation;
            }
            else 
            { 
                StopSliding(); 
            }
        }
    }

    private bool ComprobarObstaculos(Vector3 direction, float distance)
    {
        if (Physics.SphereCast(rb.position, obstacleCheckRadius, direction, out RaycastHit hit, distance + 0.1f))
        {
            // Ignorar al propio bloque y a los triggers
            if (hit.collider.gameObject != this.gameObject && !hit.collider.isTrigger)
            {
                return true;
            }
        }
        return false;
    }

    private void StopSliding()
    {
        isSliding = false;
        lateralMomentum = Vector3.zero;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}