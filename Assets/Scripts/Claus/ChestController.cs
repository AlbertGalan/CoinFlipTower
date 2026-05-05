using UnityEngine;

public class ChestController : MonoBehaviour
{
    public RouletteWheel.TipoElemento tipoCofre;
    
    [Header("Configuración de Interacción")]
    public float interactionDistance = 3f; // Distancia máxima para poder abrirlo
    
    [Header("Referencias de Objetos")]
    [Tooltip("El objeto que ya está en la escena pero desactivado.")]
    public GameObject elementInScene; 
    
    [Header("Referencias de Animación")]
    public Animator chestAnimator;   
    
    private bool opened = false;
    private Transform playerTransform;

    void Start()
    {
        // Buscamos al jugador por el tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Si el cofre ya está abierto, no hacemos nada más
        if (opened) return;

        // Si pulsamos la E...
        if (Input.GetKeyDown(KeyCode.E))
        {
            // ...y el jugador está cerca
            if (IsPlayerClose())
            {
                OpenChest();
            }
        }
    }

    private bool IsPlayerClose()
    {
        if (playerTransform == null) return false;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionDistance;
    }

    public void OpenChest()
    {
        if (opened || chestAnimator == null) return;

        opened = true;
        
        // Ejecutamos la animación de apertura (Bool isOpened = true)
        chestAnimator.SetBool("isOpened", true);
        
        // Activamos el objeto visual después de un pequeño delay
        Invoke(nameof(ActivateElement), 0.6f); 
    }

    private void ActivateElement()
    {
        if (elementInScene != null)
        {
            elementInScene.SetActive(true);
            Debug.Log($"<color=yellow>Cofre {tipoCofre}:</color> ¡Abierto y objeto activado!");
        }
    }

    // Opcional: Dibujar el radio de interacción en el Editor para que sea fácil de ajustar
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}