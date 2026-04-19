using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Guardamos esta posición como el punto de retorno
            CheckpointManager.Instance.SetCheckpoint(transform.position);
        }
    }
}