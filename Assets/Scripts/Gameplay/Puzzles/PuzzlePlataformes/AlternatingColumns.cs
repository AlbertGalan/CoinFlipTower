using UnityEngine;

public class AlternatingColumns : MonoBehaviour
{
    [Header("Límites de Movimiento")]
    public float yMin = 18f;
    public float yMax = 22f;
    
    [Header("Configuración")]
    public float speed = 2f;
    [Tooltip("Si es true, empieza en el valor Máximo. Si es false, empieza en el Mínimo.")]
    public bool startAtMax = false;

    private float timer;

    void Update()
    {
        // El timer avanza con el tiempo. 
        // Si queremos que empiece invertida, desfasamos el tiempo.
        float currentTimer = Time.time * speed;
        if (startAtMax) currentTimer += Mathf.PI; 

        // Mathf.Cos nos da un valor entre -1 y 1. 
        // Lo convertimos a un rango de 0 a 1 para el Lerp.
        float t = (Mathf.Cos(currentTimer) + 1f) / 2f;

        // Calculamos la nueva Y entre el mínimo y el máximo
        float newY = Mathf.Lerp(yMin, yMax, t);

        // Aplicamos la posición manteniendo X y Z originales
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}