using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IceBlockSlider : MonoBehaviour
{
    [Header("Ice Sliding Settings")]
    public float slideSpeedMultiplier = 1.35f;
    public float obstacleCheckRadius = 0.4f;

    private bool isSliding = false;
    private Vector3 slideDirection = Vector3.zero;
    private float baseSpeed = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Intentar obtener la velocidad base del jugador para igualarla
        MoveCharacter mc = FindFirstObjectByType<MoveCharacter>();
        if (mc != null)
        {
            baseSpeed = mc.moveSpeed;
        }
    }

    public void StartSliding(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            isSliding = true;
            slideDirection = direction.normalized;
        }
    }

    void FixedUpdate()
    {
        if (!isSliding) return;

        // Comprobar si sigue sobre el hielo
        bool overIce = false;
        RaycastHit hitDown;
        
        if (Physics.Raycast(rb.position + Vector3.up * 0.5f, Vector3.down, out hitDown, 1.5f, Physics.AllLayers, QueryTriggerInteraction.Collide))
        {
            if (hitDown.collider.CompareTag("Ice"))
            {
                overIce = true;
            }
        }

        if (!overIce)
        {
            StopSliding();
            return;
        }

        float moveDistance = (baseSpeed * slideSpeedMultiplier) * Time.fixedDeltaTime;

        // Comprobar si choca contra pared
        bool hitObstacle = false;
        Vector3 sphereOrigin = rb.position + Vector3.up * 0.5f;
        RaycastHit hitWall;

        if (Physics.SphereCast(sphereOrigin, obstacleCheckRadius, slideDirection, out hitWall, moveDistance + 0.05f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            // Ignorar colisiones consigo mismo u otros triggers
            if (hitWall.collider != GetComponent<Collider>() && hitWall.collider.isTrigger == false)
            {
                hitObstacle = true;
            }
        }

        if (hitObstacle)
        {
            StopSliding();
        }
        else
        {
            // Moverse cinemáticamente como el jugador
            Vector3 newPosition = rb.position + slideDirection * moveDistance;
            rb.MovePosition(newPosition);
        }
    }

    private void StopSliding()
    {
        isSliding = false;
        slideDirection = Vector3.zero;
        
        // Detener inercias que hubieran podido acumularse
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
