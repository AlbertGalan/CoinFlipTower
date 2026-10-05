using UnityEngine;
using System.Collections;

public class BlockPusherTrigger : MonoBehaviour
{
    public float retardoEmpuje = 1.0f;
    // Quitamos la variable de fuerza manual para usar la del jugador (-1)
    
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        IceBlockSlider slider = (rb != null) ? rb.GetComponent<IceBlockSlider>() : other.GetComponentInParent<IceBlockSlider>();

        if (slider != null && rb != null)
        {
            Debug.Log($"<color=cyan>[Pusher]</color> Detectado <b>{rb.name}</b>. Clonando acción del jugador...");
            StartCoroutine(EsperarYEmpujar(slider, rb));
        }
    }

    private IEnumerator EsperarYEmpujar(IceBlockSlider slider, Rigidbody rb)
    {
        yield return new WaitForSeconds(retardoEmpuje);

        if (slider != null && rb != null)
        {
            // 1. REPLICAR DIRECCIÓN: Usamos el forward del Trigger 
            // (Asegúrate de que la flecha azul del trigger apunte al pasillo)
            Vector3 direccionEmpuje = transform.forward;

            // 2. REPLICAR ESTADO FÍSICO: 
            // El jugador empuja un bloque que NO es kinematic.
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero; // Limpiamos caídas

            // 3. REPLICAR LLAMADA EXACTA:
            // Según tu log, el jugador envía speed: -1
            Debug.Log($"<color=green>[Pusher]</color> Ejecutando StartSliding en dirección {direccionEmpuje}");
            slider.StartSliding(direccionEmpuje, -1f); 

            // 4. EL "EMPUJONCITO" INICIAL:
            // Como el bloque está parado y el jugador suele estar moviéndose, 
            // le damos un impulso inicial para que el slider tenga "fuerza" que procesar.
            rb.AddForce(direccionEmpuje * 5f, ForceMode.Impulse);
        }
    }
}