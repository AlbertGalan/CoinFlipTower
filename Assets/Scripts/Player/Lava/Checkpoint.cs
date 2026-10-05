using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Referencia de Spawn")]
    [Tooltip("Arrastra aquí el Transform exacto donde quieres que aparezca el jugador")]
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Si has asignado un transform en el editor, usamos ese.
            // Si se te olvidó, usamos la posición del objeto actual.
            Vector3 targetPos = (spawnPoint != null) ? spawnPoint.position : transform.position;

            CheckpointManager.Instance.SetCheckpoint(targetPos);
            
            Debug.Log($"Punto de spawn actualizado a: {((spawnPoint != null) ? spawnPoint.name : gameObject.name)}");
        }
    }
}