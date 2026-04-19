using UnityEngine;
using System.Collections;

public class LavaHazard : MonoBehaviour
{
    [Header("Efectos")]
    public float fadeDuration = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RespawnSequence(other.gameObject));
        }
    }

    IEnumerator RespawnSequence(GameObject player)
    {
        // 1. Bloquear movimiento (usando tu método de MoveCharacter)
        var moveScript = player.GetComponent<MoveCharacter>();
        if (moveScript != null) moveScript.SetBlockMovement(true);

        // 2. [Opcional] Aquí dispararías un fundido a negro en la UI
        Debug.Log("Tocando lava... Teletransportando");

        yield return new WaitForSeconds(0.2f); // Pequeña pausa dramática

        // 3. Teletransporte
        // Nota: Si usas Rigidbody, es mejor usar rb.position o desactivar la física un momento
        Rigidbody rb = player.GetComponent<Rigidbody>();
        Vector3 spawnPos = CheckpointManager.Instance.GetLastCheckpoint();
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Frenar caída
            rb.position = spawnPos;
        }
        player.transform.position = spawnPos;

        yield return new WaitForSeconds(fadeDuration);

        // 4. Desbloquear movimiento
        if (moveScript != null) moveScript.SetBlockMovement(false);
    }
}