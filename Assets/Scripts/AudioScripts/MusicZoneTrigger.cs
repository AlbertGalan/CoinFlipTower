using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
public class MusicZoneTrigger : MonoBehaviour
{
    [Header("Configuración de Audio")]
    public AudioClip musicClip;
    [Range(0f, 1f)]
    public float maxVolume = 0.5f;
    public bool loop = true;

    [Header("Efecto de Transición")]
    public float fadeSpeed = 2f; // Velocidad para que la música no entre de golpe
    public bool playOnAwake = true;

    private AudioSource audioSource;
    private bool playerInside = false;
    private float targetVolume = 0f;

    void Awake()
    {
        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = loop;
        audioSource.playOnAwake = false; // Lo controlamos por script
        audioSource.volume = 0f;

        // Configurar Collider
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    void Start()
    {
        // Si el script arranca y ya estamos dentro (o queremos que suene desde el inicio)
        if (playOnAwake)
        {
            // Comprobamos si el jugador está dentro del área usando Physics
            Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2);
            foreach (var c in colliders)
            {
                if (c.CompareTag("Player"))
                {
                    playerInside = true;
                    audioSource.Play();
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Control suave del volumen (Fade In / Fade Out)
        targetVolume = playerInside ? maxVolume : 0f;

        if (audioSource.isPlaying)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * fadeSpeed);

            // Si el volumen es casi 0 y el jugador está fuera, paramos para ahorrar recursos
            if (audioSource.volume <= 0.001f && !playerInside)
            {
                audioSource.Stop();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            if (!audioSource.isPlaying) audioSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujar el área en el editor para que sea fácil de ver
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}