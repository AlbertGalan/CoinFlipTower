using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class SimonDiceManager : MonoBehaviour
{
    public enum SimonColor { Azul, Rojo, Amarillo, Verde, Morado }

    [System.Serializable]
    public class ColorData
    {
        public SimonColor color;
        public Material material;
        public AudioClip sonido;
    }

    [System.Serializable]
    public class SimonRound
    {
        public List<SimonColor> secuenciaDeEstaRonda;
    }

    [Header("Configuración de Colores")]
    public List<ColorData> listaColores;
    public Material materialGris; 

    [Header("Audio de Fallo")]
    [SerializeField] private AudioClip sonidoError; // <--- NUEVA VARIABLE
     [SerializeField] private AudioClip solveClip;

    [Header("Indicador Único")]
    public MeshRenderer indicadorUnico; 

    [Header("Configuración de Rondas")]
    public List<SimonRound> rondasConfiguradas;
    
    [Header("Tiempos")]
    public float tiempoEncendido = 0.8f;
    public float tiempoApagado = 0.4f;

    [Header("Eventos")]
    public UnityEvent OnPuzzleSolved; 
    public UnityEvent OnWrongStep;    

    private int rondaActual = 0;
    private int indiceJugador = 0;
    private bool esperandoJugador = false;
    private bool puzzleCompletado = false;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (indicadorUnico != null) indicadorUnico.material = materialGris;
    }

    public void IniciarJuego()
    {
        if (puzzleCompletado || esperandoJugador) return;
        
        rondaActual = 0;
        StartCoroutine(ReproducirSecuenciaRonda());
    }

    private IEnumerator ReproducirSecuenciaRonda()
    {
        esperandoJugador = false;
        if (indicadorUnico != null) indicadorUnico.material = materialGris;
        yield return new WaitForSeconds(1f);

        SimonRound datosRonda = rondasConfiguradas[rondaActual];

        for (int i = 0; i < datosRonda.secuenciaDeEstaRonda.Count; i++)
        {
            SimonColor colorActual = datosRonda.secuenciaDeEstaRonda[i];
            ColorData data = listaColores.Find(x => x.color == colorActual);

            if (indicadorUnico != null) indicadorUnico.material = data.material;
            if (data.sonido != null) audioSource.PlayOneShot(data.sonido);
            
            yield return new WaitForSeconds(tiempoEncendido); 

            if (indicadorUnico != null) indicadorUnico.material = materialGris;
            yield return new WaitForSeconds(tiempoApagado);
        }

        indiceJugador = 0;
        esperandoJugador = true;
        Debug.Log("Simon Dice: Turno del jugador");
    }

    public void CasillaPisada(SimonColor colorPisado)
    {
        if (!esperandoJugador || puzzleCompletado) return;

        SimonRound datosRonda = rondasConfiguradas[rondaActual];
        ColorData data = listaColores.Find(x => x.color == colorPisado);
        
        // Solo reproducimos el sonido del color si NO ha fallado todavía
        if (colorPisado == datosRonda.secuenciaDeEstaRonda[indiceJugador])
        {
            if (data.sonido != null) audioSource.PlayOneShot(data.sonido);
            indiceJugador++;

            if (indiceJugador >= datosRonda.secuenciaDeEstaRonda.Count)
            {
                rondaActual++;
                if (rondaActual >= rondasConfiguradas.Count)
                {
                    FinalizarPuzzle();
                }
                else
                {
                    StartCoroutine(ReproducirSecuenciaRonda());
                }
            }
        }
        else
        {
            Fallo();
        }
    }

    void Fallo()
    {
        esperandoJugador = false;
        if (indicadorUnico != null) indicadorUnico.material = materialGris;

        // --- LÓGICA DE AUDIO PARA FALLO ---
        if (sonidoError != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoError);
        }

        OnWrongStep?.Invoke();
        Debug.Log("Simon Dice: Fallo");
    }

    void FinalizarPuzzle()
    {
        if (solveClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(solveClip);
        }
        puzzleCompletado = true;
        esperandoJugador = false;
        if (indicadorUnico != null) indicadorUnico.material = materialGris;
        OnPuzzleSolved?.Invoke();
    }
}