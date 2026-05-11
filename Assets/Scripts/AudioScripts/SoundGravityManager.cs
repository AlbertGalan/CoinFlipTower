using UnityEngine;
using UnityEngine.Audio;

public class SoundGravityManager : MonoBehaviour
{
    // Instancia estática para acceder desde cualquier GravityController
    public static SoundGravityManager Instance; 

    [Header("Configuración de Audio")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioClip gravityUpClip;
    [SerializeField] private AudioClip gravityDownClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.75f;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Reproduce el sonido de gravedad en una posición específica.
    /// </summary>
    /// <param name="isInverted">Estado de la gravedad (true = arriba, false = abajo)</param>
    /// <param name="position">Posición del objeto que cambia de gravedad</param>
    public void PlayGravitySound(bool isInverted, Vector3 position)
    {
        AudioClip clip = isInverted ? gravityUpClip : gravityDownClip;
        
        if (clip != null)
        {
            // Creamos un objeto temporal para que el sonido sea 3D y use el Mixer
            GameObject tempGO = new GameObject("TempGravitySFX_" + (isInverted ? "Up" : "Down"));
            tempGO.transform.position = position;

            AudioSource source = tempGO.AddComponent<AudioSource>();
            source.clip = clip;
            
            // Asignación al Mixer para que el Slider de opciones funcione
            source.outputAudioMixerGroup = sfxGroup; 
            source.volume = volume;
            
            // Configuración de reproducción
            source.pitch = 1f;        // Velocidad normal
            source.spatialBlend = 1f; // Sonido 3D (se atenúa con la distancia)

            source.Play();

            // Se destruye automáticamente cuando el clip termina de sonar
            Destroy(tempGO, clip.length);
        }
        else
        {
            Debug.LogWarning("SoundGravityManager: No se ha asignado el AudioClip para " + (isInverted ? "Gravity Up" : "Gravity Down"));
        }
    }
}