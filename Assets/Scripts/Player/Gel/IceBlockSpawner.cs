using UnityEngine;

public class IceBlockSpawner : MonoBehaviour
{
    [Header("Configuración del Prefab")]
    public GameObject prefabBloqueHielo;
    public Transform puntoSpawn;

    [Header("Ajustes de Disparo")]
    public float fuerzaDisparo = 15f; 
    public float intervaloSpawn = 2f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= intervaloSpawn)
        {
            DispararBloque();
            timer = 0;
        }
    }

    void DispararBloque()
    {
        // 1. Instanciamos el bloque
        GameObject nuevoBloque = Instantiate(prefabBloqueHielo, puntoSpawn.position, puntoSpawn.rotation);
        
        Rigidbody rb = nuevoBloque.GetComponent<Rigidbody>();
        IceBlockSlider slider = nuevoBloque.GetComponent<IceBlockSlider>();

        if (rb != null)
        {
            // Nos aseguramos de que no sea cinemático para que la fuerza le afecte
            rb.isKinematic = false;

            // 2. Aplicamos la fuerza en el eje Z local del Spawner
            // ForceMode.Impulse es ideal para disparos instantáneos
            Vector3 direccionZ = transform.forward; 
            rb.AddForce(direccionZ * fuerzaDisparo, ForceMode.Impulse);
            
            // 3. Iniciamos el sistema de deslizamiento
            // Le pasamos la dirección para que el script sepa hacia dónde "resbalar"
            if (slider != null)
            {
                slider.StartSliding(direccionZ);
            }
        }
    }

    // Para ver en el editor hacia dónde disparará (flecha azul)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(puntoSpawn != null ? puntoSpawn.position : transform.position, transform.forward * 3f);
    }
}