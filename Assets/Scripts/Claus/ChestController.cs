using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Asegura que haya un AudioSource
public class ChestController : MonoBehaviour
{
    public RouletteWheel.TipoElemento tipoCofre;
    
    [Header("Configuración de Interacción")]
    public float interactionDistance = 3f; 
    
    [Header("Referencias de Objetos")]
    public GameObject elementInScene; 
    
    [Header("Referencias de Animación")]
    public Animator chestAnimator;   

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    
    private bool opened = false;
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Si no asignaste el AudioSource en el inspector, lo buscamos
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // Se eliminó el Update para que no use la tecla 'E'

    private bool IsPlayerClose()
    {
        if (playerTransform == null) return false;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionDistance;
    }

    public void OpenChest()
    {
        // Solo abrimos si no está abierto Y el jugador está en rango
        if (opened || chestAnimator == null || !IsPlayerClose()) return;

        opened = true;
        
        // Animación
        chestAnimator.SetBool("isOpened", true);

        // Sonido
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        Invoke(nameof(ActivateElement), 0.6f); 
    }

    private void ActivateElement()
    {
        if (elementInScene != null)
        {
            elementInScene.SetActive(true);
            Debug.Log($"<color=yellow>Cofre {tipoCofre}:</color> ¡Abierto con magia!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}