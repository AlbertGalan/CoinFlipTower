using System.Collections;
using UnityEngine;

public class RouletteWheel : MonoBehaviour
{
    [Header("Configuración de Giro")]
    public float tiempoGiro = 3.5f;
    public int vueltasMinimas = 4;
    
    private bool estaGirando = false;
    private const float constanteY = -90f;
    private const float constanteZ = 90f;

    public enum TipoElemento { Terra, Gel, Foc }

    private PlayerRouteData playerLog;

    private void Start()
    {
        playerLog = FindFirstObjectByType<PlayerRouteData>();
    }

    public void Spin()
    {
        if (estaGirando) return;

        // La ruleta SIEMPRE es aleatoria, no importa si es el giro 1 o el 100
        int suerte = Random.Range(0, 3); 
        TipoElemento resultado = (TipoElemento)suerte;

        float anguloDestinoX = CalcularAnguloSegunRango(resultado);
        StartCoroutine(AnimarGiro(anguloDestinoX, resultado));
    }

    private float CalcularAnguloSegunRango(TipoElemento tipo)
    {
        switch (tipo)
        {
            case TipoElemento.Terra: return Random.Range(-91f, -209f);
            case TipoElemento.Gel:  return Random.Range(-211f, -329f);
            case TipoElemento.Foc:  return Random.Range(-331f, -449f);
            default: return -90f;
        }
    }

    private IEnumerator AnimarGiro(float anguloFinalX, TipoElemento resultado)
    {
        estaGirando = true;
        float tiempoPasado = 0;
        float rotacionInicialX = transform.eulerAngles.x;
        float destinoTotalX = anguloFinalX - (vueltasMinimas * 360f);

        while (tiempoPasado < tiempoGiro)
        {
            tiempoPasado += Time.deltaTime;
            float t = tiempoPasado / tiempoGiro;
            float suavizado = 1f - Mathf.Pow(1f - t, 3f);
            float xActual = Mathf.Lerp(rotacionInicialX, destinoTotalX, suavizado);
            transform.eulerAngles = new Vector3(xActual, constanteY, constanteZ);
            yield return null;
        }

        transform.eulerAngles = new Vector3(anguloFinalX, constanteY, constanteZ);
        estaGirando = false;

        // Intentamos enviar el resultado al jugador
        if (playerLog != null)
        {
            // El script del jugador se encarga de ignorarlo si ya tiene una ruta
            playerLog.SetRuta(resultado);
        }

        Debug.Log("<color=yellow>Ruleta Aturada!</color> Resultado visual: **" + resultado + "**");
    }
}