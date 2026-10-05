using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RouletteWheel : MonoBehaviour
{
    [Header("Configuración de Giro")]
    public float tiempoGiro = 3.5f;
    public int vueltasMinimas = 4;
    
    [Header("Audio")]
    [Tooltip("Fuente de audio que reproducirá el sonido de giro")]
    public AudioSource audioSource;
    [Tooltip("Clip de sonido de la ruleta girando (clic-clic-clic)")]
    public AudioClip spinClip;

    [Header("Eventos")]
    [Tooltip("Evento que se ejecutará SOLO la primera vez que la ruleta dé un resultado")]
    public UnityEvent onFirstResult;

    private bool estaGirando = false;
    private const float constanteY = -90f;
    private const float constanteZ = 90f;

    public enum TipoElemento { Terra, Gel, Foc }
    private PlayerRouteData playerLog;
    private bool primerResultadoDisparado = false;

    private void Start()
    {
        playerLog = FindFirstObjectByType<PlayerRouteData>();
        primerResultadoDisparado = false;

        // Intentar auto-asignar el AudioSource si no está en el inspector
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        // Configuración inicial del audio para que no falle
        if (audioSource != null && spinClip != null)
        {
            audioSource.clip = spinClip;
            audioSource.loop = true; // Queremos que el sonido se repita mientras gira
            audioSource.playOnAwake = false;
        }
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
        
        // --- INICIO DEL SONIDO ---
        if (audioSource != null && spinClip != null)
        {
            audioSource.Play();
        }

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

        // --- FIN DEL SONIDO ---
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        CheckFirstTimeEvent();

        if (playerLog != null)
        {
            playerLog.SetRuta(resultado);
        }

        Debug.Log("<color=yellow>Ruleta Aturada!</color> Resultado visual: **" + resultado + "**");
    }

    private void CheckFirstTimeEvent()
    {
        if (!primerResultadoDisparado)
        {
            if (onFirstResult != null)
            {
                onFirstResult.Invoke();
            }

            primerResultadoDisparado = true;
        }
    }
    
    public void ResetFirstSpinStatus()
    {
        primerResultadoDisparado = false;
    }
}