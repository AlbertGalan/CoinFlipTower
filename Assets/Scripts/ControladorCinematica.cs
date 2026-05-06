using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro; // Necesario para el texto
using System.Collections;

public class ControladorCinematica : MonoBehaviour
{
    [Header("Configuración Vídeo")]
    public VideoPlayer videoPlayer;
    public string nombreSiguienteEscena;

    [Header("UI Saltar Cinemática")]
    public GameObject objetoTextoSkip; // El objeto que contiene el texto
    public CanvasGroup canvasGroupTexto; // Para el efecto de parpadeo
    public float tiempoVisible = 3f; // Cuánto tarda en desaparecer el aviso
    public float velocidadParpadeo = 2f;

    private bool avisoActivo = false;
    private float timerDesaparecer;

    void Start()
    {
        videoPlayer.loopPointReached += AlTerminarVideo;

        // Aseguramos que el texto esté oculto al empezar
        if (canvasGroupTexto != null)
        {
            canvasGroupTexto.alpha = 0;
            objetoTextoSkip.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Detectar si pulsamos CUALQUIER tecla para mostrar el aviso
        if (Input.anyKeyDown && !avisoActivo)
        {
            // Ojo: Si pulsamos Espacio directamente cuando NO hay aviso, 
            // el Input.anyKeyDown se activará, pero queremos que primero muestre el texto.
            MostrarAviso();
        }
        // 2. Si el aviso YA está activo y pulsamos ESPACIO, saltamos
        else if (avisoActivo && Input.GetKeyDown(KeyCode.Space))
        {
            CargarSiguienteEscena();
        }

        // 3. Lógica de temporizador para ocultar el aviso
        if (avisoActivo)
        {
            ManejarAviso();
        }
    }

    private void MostrarAviso()
    {
        avisoActivo = true;
        objetoTextoSkip.SetActive(true);
        timerDesaparecer = tiempoVisible;
    }

    private void ManejarAviso()
    {
        // Hacer que el texto parpadee usando una onda Seno
        float alpha = (Mathf.Sin(Time.time * velocidadParpadeo) + 1f) / 2f;
        canvasGroupTexto.alpha = alpha;

        // Temporizador para desaparecer
        timerDesaparecer -= Time.deltaTime;
        if (timerDesaparecer <= 0)
        {
            OcultarAviso();
        }
    }

    private void OcultarAviso()
    {
        avisoActivo = false;
        canvasGroupTexto.alpha = 0;
        objetoTextoSkip.SetActive(false);
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        CargarSiguienteEscena();
    }

    public void CargarSiguienteEscena()
    {
        // Es buena práctica quitar la suscripción al evento antes de cambiar de escena
        videoPlayer.loopPointReached -= AlTerminarVideo;
        SceneManager.LoadScene(nombreSiguienteEscena);
    }
}