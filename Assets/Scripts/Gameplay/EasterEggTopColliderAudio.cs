using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EasterEggTopColliderAudio : MonoBehaviour
{
    [Header("Collision Filter")]
    [Tooltip("Solo reacciona cuando el objeto que entra/colisiona tiene esta etiqueta")]
    public string requiredTag = "EasterEgg";

    [Header("Audio")]
    [Tooltip("AudioSource que sonará al tocar este collider")]
    public AudioSource audioSource;

    [Tooltip("Duración antes de destruir este objeto (ej: 7s)")]
    public float destroyAfterSeconds = 7f;

    [Header("Actions")]
    [Tooltip("GameObject a activar cuando entra por OnCollisionEnter")]
    public GameObject objectToActivateOnCollision;

    [Tooltip("Evita ejecutar esta lógica más de una vez")]
    public bool triggerOnlyOnce = true;

    [Tooltip("Si está activo, desactiva ESTE collider después de activarse")]
    public bool disableThisColliderAfterTrigger = false;

    private bool hasTriggered;
    private Collider ownCollider;

    private void Awake()
    {
        ownCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject, false);
    }

    private void HandleHit(GameObject other, bool fromCollision)
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (!other.CompareTag(requiredTag))
            return;

        GravityController gravityController = other.GetComponent<GravityController>();
        if (gravityController != null)
        {
            gravityController.LockGravityChanges();
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (fromCollision && objectToActivateOnCollision != null)
        {
            objectToActivateOnCollision.SetActive(true);
        }

        hasTriggered = true;

        if (disableThisColliderAfterTrigger && ownCollider != null)
        {
            ownCollider.enabled = false;
        }

        if (destroyAfterSeconds <= 0f)
            Destroy(gameObject);
        else
            Destroy(gameObject, destroyAfterSeconds);
    }
}