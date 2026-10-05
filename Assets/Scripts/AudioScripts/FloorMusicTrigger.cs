using UnityEngine;

public class FloorMusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip floorClip;
    [SerializeField] private float fadeDuration = 1.5f;
    
    private MusicFloorManager musicManager; // Variable para guardar la referencia

    private void Start()
    {
        // Buscamos el manager solo una vez al inicio
        musicManager = Object.FindFirstObjectByType<MusicFloorManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si es el jugador y si tenemos el manager
        if (other.CompareTag("Player") && musicManager != null)
        {
            musicManager.SwitchTrack(floorClip, fadeDuration);
        }
    }
}