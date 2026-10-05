using UnityEngine;
using UnityEngine.Events;

public class ElementItem : MonoBehaviour
{
    public RouletteWheel.TipoElemento tipo;
    public GameObject portalToActivate;
    public UnityEvent onCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Buscamos el componente de datos en el jugador
            PlayerRouteData data = other.GetComponent<PlayerRouteData>();
            
            if (data != null)
            {
                data.RecogerObjeto(tipo); // Le damos el objeto al jugador
            }

            onCollected?.Invoke();

            if (portalToActivate != null) portalToActivate.SetActive(true);
            
            Destroy(gameObject);
        }
    }
}