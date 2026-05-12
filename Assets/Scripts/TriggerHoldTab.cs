using UnityEngine;

public class TriggerHoldTab : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject objectToShow; // El objeto que quieres mostrar/ocultar

    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";

    private bool isPlayerInside = false;

    private void Start()
    {
        // Asegurarnos de que el objeto empiece oculto
        if (objectToShow != null)
        {
            objectToShow.SetActive(false);
        }
    }

    private void Update()
    {
        // Solo comprobamos la tecla si el jugador está dentro del trigger
        if (isPlayerInside)
        {
            // Input.GetKey devuelve true MIENTRAS mantienes pulsada la tecla
            if (Input.GetKey(KeyCode.Tab))
            {
                if (!objectToShow.activeSelf) objectToShow.SetActive(true);
            }
            else
            {
                if (objectToShow.activeSelf) objectToShow.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            
            // Forzamos que se oculte al salir, por si el jugador 
            // sale del trigger manteniendo TAB pulsado
            if (objectToShow != null)
            {
                objectToShow.SetActive(false);
            }
        }
    }
}