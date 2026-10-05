using UnityEngine;
using System.Collections;

public class IceBlockSpawner : MonoBehaviour
{
    [Header("Configuración del Prefab")]
    public GameObject prefabBloqueHielo;
    public Transform puntoSpawn;

    [Header("Ajustes de Tiempo")]
    public float intervaloSpawn = 3f;
    public float retardoEmpuje = 1.0f; // El segundo de cortesía

    private float timer;

    void Update()
    {
        if (Time.timeScale == 0f) return;

        timer += Time.deltaTime;
        if (timer >= intervaloSpawn)
        {
            SpawnearBloque();
            timer = 0;
        }
    }

    void SpawnearBloque()
    {
        // 1. Instanciar
        GameObject nuevoBloque = Instantiate(prefabBloqueHielo, puntoSpawn.position, puntoSpawn.rotation);
        
        Rigidbody rb = nuevoBloque.GetComponent<Rigidbody>();
        IceBlockSlider slider = nuevoBloque.GetComponent<IceBlockSlider>();

        if (rb != null && slider != null)
        {
            // 2. Configuración física inicial
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 3. Iniciamos la cuenta atrás para el empuje desde aquí mismo
            StartCoroutine(EsperarYEmpujar(slider, rb));
        }
        else
        {
            Debug.LogWarning("[Spawner] El prefab no tiene Rigidbody o IceBlockSlider.");
        }
    }

    private IEnumerator EsperarYEmpujar(IceBlockSlider slider, Rigidbody rb)
    {
        // Esperamos a que el bloque se asiente en el suelo
        yield return new WaitForSeconds(retardoEmpuje);

        if (slider != null && rb != null)
        {
            // Usamos el forward del PUNTO DE SPAWN como dirección
            Vector3 direccionEmpuje = puntoSpawn.forward;

            // Replicamos la acción exacta del jugador
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;

            // Llamada al slider (con el speed -1 que ya sabemos que funciona)
            slider.StartSliding(direccionEmpuje, -1f); 

            // El impulso inicial para romper la inercia
            rb.AddForce(direccionEmpuje * 5f, ForceMode.Impulse);

            //Debug.Log($"<color=green>[Spawner]</color> Bloque {rb.name} empujado con éxito.");
        }
    }

    void OnDrawGizmos()
    {
        if (puntoSpawn == null) return;
        
        // Esfera de spawn
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(puntoSpawn.position, 0.3f);
        
        // Flecha de dirección de empuje (Z azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(puntoSpawn.position, puntoSpawn.forward * 2f);
    }
}