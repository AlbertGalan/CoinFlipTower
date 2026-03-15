using UnityEngine;

public class EasterEggCollisionAction : MonoBehaviour
{
    [Header("Collision Filter")]
    [Tooltip("Solo reacciona cuando el objeto que colisiona tiene esta etiqueta")]
    public string requiredTag = "EasterEgg";

    [Header("Move Up (GravityController)")]
    [Tooltip("GravityController del objeto que quieres mover hacia arriba")]
    public GravityController gravityControllerToMove;

    [Tooltip("Si está activo, pone la gravedad invertida para que suba")]
    public bool moveUpOnCollision = true;

    [Header("Audio")]
    [Tooltip("AudioSource que sonará al aplicar el movimiento hacia arriba")]
    public AudioSource moveAudioSource;

    [Tooltip("Reinicia el audio si ya estaba sonando")]
    public bool restartAudioIfPlaying = true;

    [Header("Actions")]
    [Tooltip("Objeto a activar cuando ocurre la colisión")]
    public GameObject objectToActivate;

    [Tooltip("Objeto a destruir cuando ocurre la colisión")]
    public GameObject objectToDestroy;

    [Tooltip("Evita ejecutar la acción más de una vez")]
    public bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void OnCollisionEnter(Collision collision)
    {
        TryExecute(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryExecute(other.gameObject);
    }

    private void TryExecute(GameObject otherObject)
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (!otherObject.CompareTag(requiredTag))
            return;

        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (objectToDestroy != null)
            Destroy(objectToDestroy);

        bool movedUp = false;
        if (moveUpOnCollision && gravityControllerToMove != null)
        {
            gravityControllerToMove.SetGravityInverted(true);
            movedUp = true;
        }

        if (movedUp && moveAudioSource != null)
        {
            if (restartAudioIfPlaying && moveAudioSource.isPlaying)
                moveAudioSource.Stop();

            moveAudioSource.Play();
        }

        hasTriggered = true;
    }
}