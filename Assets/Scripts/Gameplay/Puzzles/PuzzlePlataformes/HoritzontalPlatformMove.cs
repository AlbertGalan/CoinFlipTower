using UnityEngine;

public class HoritzontalPlatformMove : MonoBehaviour
{
    [Header("Límites de Movimiento (Eje X)")]
    public float xMin;
    public float xMax;
    
    [Header("Configuración")]
    public float speed = 2f;
    [Tooltip("Si es true, empieza en el valor máximo de X.")]
    public bool startAtMax = false;

    void Update()
    {
        // Calculamos el ciclo de movimiento
        float currentTimer = Time.time * speed;
        if (startAtMax) currentTimer += Mathf.PI; 

        // Suavizado con Coseno (aceleración/frenado en extremos)
        float t = (Mathf.Cos(currentTimer) + 1f) / 2f;

        // Interpolamos la posición en X
        float newX = Mathf.Lerp(xMin, xMax, t);

        // Aplicamos la posición manteniendo Y y Z actuales
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}