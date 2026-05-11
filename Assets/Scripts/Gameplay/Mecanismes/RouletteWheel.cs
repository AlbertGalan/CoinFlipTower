using System.Collections;
using UnityEngine;
using UnityEngine.Events; // Necesario para UnityEvent

public class RouletteWheel : MonoBehaviour
{
    [Header("Configuración de Giro")]
    public float tiempoGiro = 3.5f;
    public int vueltasMinimas = 4;
    
    [Header("Eventos")]
    [Tooltip("Evento que se ejecutará SOLO la primera vez que la ruleta dé un resultado")]
    public UnityEvent onFirstResult;

    private bool estaGirando = false;
    private const float constanteY = -90f;
    private const float constanteZ = 90f;

    public enum TipoElemento { Terra, Gel, Foc }
    private PlayerRouteData playerLog;

    // Clave para guardar en memoria si ya se activó el primer resultado
    private const string FirstSpinKey = "RouletteFirstSpinDone";

    private void Start()
    {
        playerLog = FindFirstObjectByType<PlayerRouteData>();
    }

    public void Spin()
    {
        if (estaGirando) return;

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

        if (playerLog != null)
        {
            playerLog.SetRuta(resultado);
        }

        // --- LÓGICA DEL PRIMER RESULTADO ---
        CheckFirstTimeEvent();

        Debug.Log("<color=yellow>Ruleta Aturada!</color> Resultado visual: **" + resultado + "**");
    }

    private void CheckFirstTimeEvent()
    {
        // Comprobamos si ya se ha disparado antes (usando PlayerPrefs para que sea persistente)
        if (PlayerPrefs.GetInt(FirstSpinKey, 0) == 0)
        {
            Debug.Log("<color=cyan>Ruleta:</color> ¡Primer resultado detectado! Disparando evento...");
            
            // Disparamos el evento de Unity
            if (onFirstResult != null)
            {
                onFirstResult.Invoke();
            }

            // Marcamos como hecho para que no vuelva a ocurrir
            PlayerPrefs.SetInt(FirstSpinKey, 1);
            PlayerPrefs.Save();
        }
    }
    
    // Método extra por si necesitas resetear esta lógica desde otro script o botón de debug
    public void ResetFirstSpinStatus()
    {
        PlayerPrefs.SetInt(FirstSpinKey, 0);
        PlayerPrefs.Save();
    }
}