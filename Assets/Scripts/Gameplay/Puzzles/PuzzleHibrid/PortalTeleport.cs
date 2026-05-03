using UnityEngine;
using System.Collections;

public class PortalTeleport : MonoBehaviour
{
    [Header("Destino")]
    public Transform targetDestination; // El GameObject vacío donde aparecerá el jugador

    [Header("Efectos")]
    public bool useFade = true; // Por si quieres añadir un fundido a negro luego
    public float teleportDelay = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos que sea el jugador
        if (other.CompareTag("Player"))
        {
            StartCoroutine(TeleportSequence(other.gameObject));
        }
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        if (targetDestination == null)
        {
            Debug.LogError("¡No has asignado un destino al portal!");
            yield break;
        }

        // 1. Bloqueamos movimiento (opcional, reutilizando tu MoveCharacter)
        var moveScript = player.GetComponent<MoveCharacter>();
        if (moveScript != null) moveScript.SetBlockMovement(true);

        yield return new WaitForSeconds(teleportDelay);

        // 2. Referencia al Rigidbody para resetear físicas
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Importante: Ponemos la velocidad a 0 para que no mantenga la inercia de la sala anterior
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Usamos rb.position para que el motor de físicas no intente interpolar el movimiento
            rb.position = targetDestination.position;
        }
        
        // 3. Movemos el transform por seguridad
        player.transform.position = targetDestination.position;
        
        // 4. Sincronizamos la rotación (para que el jugador mire hacia donde apunta el targetDestination)
        player.transform.rotation = targetDestination.rotation;

        yield return new WaitForSeconds(0.1f);

        // 5. Desbloqueamos movimiento
        if (moveScript != null) moveScript.SetBlockMovement(false);

        Debug.Log("Teletransporte completado a: " + targetDestination.name);
    }
}