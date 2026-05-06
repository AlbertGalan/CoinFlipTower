using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Si el bloque toca los pinchos, desaparece
        if (other.CompareTag("IceBlock"))
        {
            // Podrías spawnear partículas de hielo rompiéndose aquí
            Destroy(other.gameObject);
            Debug.Log("Bloque destruido por pinchos");
        }
    }
}