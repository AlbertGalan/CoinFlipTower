using UnityEngine;

public class DistanceObjectLayer : MonoBehaviour
{
    [Header("Configuración de Distancia")]
    public float interactionDistance = 3f;

    [Header("Configuración de Capas")]
    public string grabbableLayerName = "Grabbable Layer";
    public string defaultLayerName = "Default";

    [Header("Objetos a Afectar")]
    [Tooltip("El objeto que cambiará de capa (y sus hijos). Si es nulo, usa este GameObject.")]
    public GameObject targetObject;

    private int grabbableLayer;
    private int defaultLayer;
    private Transform playerTransform;

    private void Start()
    {
        if (targetObject == null) targetObject = this.gameObject;

        // Convertir nombres a índices (mucho más rápido que comparar strings)
        grabbableLayer = LayerMask.NameToLayer(grabbableLayerName);
        defaultLayer = LayerMask.NameToLayer(defaultLayerName);

        // Validación de capas
        if (grabbableLayer == -1 || defaultLayer == -1)
        {
            Debug.LogError($"<color=red>Error de Capas en {gameObject.name}:</color> Revisa que los nombres de las capas sean correctos en el Inspector.");
        }

        FindPlayerInstance();
    }

    private void FindPlayerInstance()
    {
        // Prioridad 1: Buscar por componente MoveCharacter
        MoveCharacter moveScript = FindFirstObjectByType<MoveCharacter>();
        if (moveScript != null)
        {
            playerTransform = moveScript.transform;
            return;
        }

        // Prioridad 2: Buscar por Tag
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
        }
    }

    private void Update()
    {
        // Si el jugador no existe (o aún no ha cargado), no hacemos nada
        if (playerTransform == null)
        {
            FindPlayerInstance();
            return;
        }

        // Cálculo de distancia (con 11 objetos es insignificante para la CPU)
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance <= interactionDistance)
        {
            if (targetObject.layer != grabbableLayer)
            {
                SetLayerRecursive(targetObject.transform, grabbableLayer);
            }
        }
        else
        {
            if (targetObject.layer != defaultLayer)
            {
                SetLayerRecursive(targetObject.transform, defaultLayer);
            }
        }
    }

    private void SetLayerRecursive(Transform target, int layer)
    {
        if (target == null) return;
        
        target.gameObject.layer = layer;
        foreach (Transform child in target)
        {
            SetLayerRecursive(child, layer);
        }
    }

    // Dibujamos el rango en el editor para que sea fácil de ajustar
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}