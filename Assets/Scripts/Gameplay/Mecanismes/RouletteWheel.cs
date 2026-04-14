using System.Collections;
using UnityEngine;

public class RouletteWheel : MonoBehaviour
{
    [Header("Configuración de Giro")]
    public float tiempoGiro = 3.5f;
    public int vueltasMinimas = 4;
    
    private bool estaGirando = false;

    // Los valores fijos de Y y Z de tu Inspector
    private const float constanteY = -90f;
    private const float constanteZ = 90f;

    public enum TipoElemento { Tierra, Hielo, Fuego }

    public void Spin()
    {
        if (estaGirando) return;

        // 1. Decidir resultado (33% Tierra, 34% Hielo, 33% Fuego aprox)
        int suerte = Random.Range(0, 3); 
        TipoElemento resultado = (TipoElemento)suerte;

        // 2. Obtener un ángulo aleatorio dentro de TUS rangos calculados
        float anguloDestinoX = CalcularAnguloSegunRango(resultado);

        // 3. Iniciar Corrutina de movimiento
        StartCoroutine(AnimarGiro(anguloDestinoX, resultado));
    }

    private float CalcularAnguloSegunRango(TipoElemento tipo)
    {
        switch (tipo)
        {
            // Usamos tus rangos exactos:
            case TipoElemento.Tierra: return Random.Range(-91f, -209f);
            case TipoElemento.Hielo:  return Random.Range(-211f, -329f);
            case TipoElemento.Fuego:  return Random.Range(-331f, -449f);
            default: return -90f;
        }
    }

    private IEnumerator AnimarGiro(float anguloFinalX, TipoElemento resultado)
    {
        estaGirando = true;
        float tiempoPasado = 0;
        
        float rotacionInicialX = transform.eulerAngles.x;
        
        // Calculamos el destino sumando las vueltas completas (en negativo por tus rangos)
        float destinoTotalX = anguloFinalX - (vueltasMinimas * 360f);

        while (tiempoPasado < tiempoGiro)
        {
            tiempoPasado += Time.deltaTime;
            float t = tiempoPasado / tiempoGiro;
            
            // Suavizado tipo "ease out": gira más rápido al principio y se frena al final.
            float suavizado = 1f - Mathf.Pow(1f - t, 3f);

            float xActual = Mathf.Lerp(rotacionInicialX, destinoTotalX, suavizado);
            
            // Aplicamos la rotación respetando tus ejes Y y Z
            transform.eulerAngles = new Vector3(xActual, constanteY, constanteZ);

            yield return null;
        }

        // Ajuste final de precisión
        transform.eulerAngles = new Vector3(anguloFinalX, constanteY, constanteZ);
        
        estaGirando = false;
        Debug.Log("<color=yellow>¡Ruleta detenida!</color> Ha tocado: **" + resultado + "**");
    }
}