using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(IceBlockSlider))]
public class IceSliderAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip iceSlideClip;
    [Range(0f, 1f)]
    public float maxVolume = 0.7f;
    public float fadeSpeed = 5f;

    private AudioSource audioSource;
    private IceBlockSlider sliderScript;
    private float targetVolume = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sliderScript = GetComponent<IceBlockSlider>();

        // Configuración automática del AudioSource
        audioSource.clip = iceSlideClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    void Update()
    {
        bool isSliding = IsObjectSliding();

        if (isSliding)
        {
            targetVolume = maxVolume;
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            targetVolume = 0f;
        }

        // Aplicamos un suavizado (Fade) al volumen para que suene natural
        if (audioSource.isPlaying)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * fadeSpeed);

            // Si el volumen llega a 0 y no estamos deslizando, paramos el AudioSource
            if (audioSource.volume <= 0.01f && !isSliding)
            {
                audioSource.Stop();
            }
        }
    }

    private bool IsObjectSliding()
    {
        // Primero intentamos con IceBlockSlider (para bloques)
        if (sliderScript != null && sliderScript.enabled)
        {
            return sliderScript.IsSliding;
        }

        // Si no está disponible, buscamos MoveCharacter (para el personaje principal)
        MoveCharacter moveCharacter = GetComponent<MoveCharacter>();
        if (moveCharacter != null && moveCharacter.enabled)
        {
            return moveCharacter.IsSliding;
        }

        return false;
    }
}