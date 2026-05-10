using UnityEngine;

public class SpecialGrabbableObject : MonoBehaviour
{
    [Header("Configuración de Respawn")]
    public Transform spawnPoint;

    [Header("Detección de Distancia")]
    [Tooltip("Distancia a la que el objeto se vuelve 'agarrable'")]
    public float interactionDistance = 4f;
    [Tooltip("La capa para que el PushPullController lo detecte")]
    public string grabbableLayerName = "GrabbableLayer";

    private Transform playerTransform;
    private Rigidbody rb;
    private GravityController gravityScript;
    private int defaultLayer;
    private int grabbableLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityScript = GetComponent<GravityController>();
        
        // Cache de capas y jugador
        playerTransform = Camera.main.transform; // O usa un tag "Player"
        defaultLayer = LayerMask.NameToLayer("Default");
        grabbableLayer = LayerMask.NameToLayer(grabbableLayerName);

        // Forzar gravedad fija al inicio
        if (gravityScript != null)
        {
            gravityScript.gravityInverted = true;
            gravityScript.UpdateGravityDirection();
            gravityScript.LockGravityChanges();
        }

        // Empezar en Default
        gameObject.layer = defaultLayer;
    }

    void Update()
    {
        CheckDistanceToPlayer();
    }

    private void CheckDistanceToPlayer()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Si estamos cerca, pasamos a GrabbableLayer
        if (distance <= interactionDistance)
        {
            if (gameObject.layer != grabbableLayer)
            {
                gameObject.layer = grabbableLayer;
                Debug.Log($"<color=green>{gameObject.name}:</color> Cerca del jugador, activando capa de agarre.");
            }
        }
        // Si nos alejamos, volvemos a Default
        else
        {
            if (gameObject.layer != defaultLayer)
            {
                gameObject.layer = defaultLayer;
                Debug.Log($"<color=red>{gameObject.name}:</color> Lejos del jugador, desactivando capa de agarre.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LavaHazard>() != null)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (spawnPoint == null) return;

        if (rb != null)
        {
            #if UNITY_2023_1_OR_NEWER
            rb.linearVelocity = Vector3.zero;
            #else
            rb.velocity = Vector3.zero;
            #endif
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        if (gravityScript != null)
        {
            gravityScript.gravityInverted = true;
            gravityScript.UpdateGravityDirection();
        }
    }
}