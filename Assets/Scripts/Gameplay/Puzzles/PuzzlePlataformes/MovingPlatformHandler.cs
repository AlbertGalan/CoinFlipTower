using UnityEngine;

public class MovingPlatformHandler : MonoBehaviour
{
    private Vector3 lastPosition;
    private Vector3 platformVelocity;

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Calculamos cuánto se ha movido la plataforma en este frame físico
        platformVelocity = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        // Si el jugador está dentro de la zona del Trigger
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Movemos la posición del Rigidbody del jugador 
                // sumándole el desplazamiento de la plataforma
                playerRb.MovePosition(playerRb.position + platformVelocity);
            }
        }
    }
}