using UnityEngine;
using UnityEngine.Events;

public class IceContainer : MonoBehaviour
{
    public bool estaLleno = false;
    public string tagBloque = "IceBlock"; // Asegúrate de que el prefab tenga este Tag
    
    [Header("Efectos")]
    public GameObject efectoVisualLleno;

    // Evento interno para avisar al puzzle
    [HideInInspector] public UnityEvent onBlockEntered = new UnityEvent();

    private void OnTriggerEnter(Collider other)
    {
        if (!estaLleno && other.CompareTag(tagBloque))
        {
            estaLleno = true;
            
            // Opcional: Destruir el bloque o dejarlo fijo dentro
            Destroy(other.gameObject); 

            if (efectoVisualLleno != null) efectoVisualLleno.SetActive(true);
            
            onBlockEntered.Invoke();
            Debug.Log(gameObject.name + " llenado!");
        }
    }
}