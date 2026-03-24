using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class ControladorCinematica : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nombreSiguienteEscena;

    void Start()
    {
        // Nos suscribimos al evento que se activa cuando el video termina
        videoPlayer.loopPointReached += AlTerminarVideo;
    }

    void Update()
    {
        // Si el jugador presiona cualquier tecla o botón asignado
        if (Input.anyKeyDown)
        {
            CargarSiguienteEscena();
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        CargarSiguienteEscena();
    }

    public void CargarSiguienteEscena()
    {
        SceneManager.LoadScene(nombreSiguienteEscena);
    }
}