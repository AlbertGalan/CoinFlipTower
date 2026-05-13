using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;
    private Vector3 lastCheckpointPosition;

    public AudioClip lastCheckpointSound;

    public AudioSource audioSource;
    void Awake()
    {
        if (Instance == null) Instance = this;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) lastCheckpointPosition = player.transform.position;
    }

    public void SetCheckpoint(Vector3 pos) => lastCheckpointPosition = pos;

    public void RespawnPlayer(GameObject player)
    {
            if (lastCheckpointSound != null)
            {
                AudioSource.PlayClipAtPoint(lastCheckpointSound, player.transform.position);
            }
        Debug.Log($"<color=cyan>--- INICIO RESPAWN ---</color>");
        Debug.Log($"Posición deseada: {lastCheckpointPosition}");

        Rigidbody rb = player.GetComponent<Rigidbody>();
        CharacterController cc = player.GetComponent<CharacterController>();

        // 1. Detección de CharacterController (Suelen bloquear transform.position)
        if (cc != null)
        {
            Debug.Log("<color=orange>Debug:</color> CharacterController detectado. Desactivando para mover...");
            cc.enabled = false;
        }

        // 2. Parar Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; 
            Debug.Log($"<color=orange>Debug:</color> Rigidbody puesto en Kinematic. Vel actual: {rb.linearVelocity}");
        }

        // 3. MOVIMIENTO
        Vector3 posAntes = player.transform.position;
        player.transform.position = lastCheckpointPosition;
        Physics.SyncTransforms(); // Sincronización de motor de colisiones
        
        Debug.Log($"Movimiento Transform: {posAntes} -> {player.transform.position}");

        // 4. REACTIVACIÓN
        if (rb != null) rb.isKinematic = false;
        if (cc != null) cc.enabled = true;

        // Comprobación final después de un frame de física (opcional pero útil)
        Debug.Log($"<color=cyan>--- FIN RESPAWN ---</color>");
    }
}