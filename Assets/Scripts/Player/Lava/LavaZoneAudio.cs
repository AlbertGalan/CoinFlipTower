using UnityEngine;
using System.Collections;

public class LavaZoneAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource lavaSource;
    public float maxVolume = 0.7f;
    public float fadeDuration = 1.5f;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (lavaSource != null)
        {
            lavaSource.volume = 0;
            lavaSource.loop = true;
            lavaSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartFade(maxVolume);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartFade(0f);
        }
    }

    private void StartFade(float targetVolume)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(targetVolume));
    }

    private IEnumerator FadeAudio(float target)
    {
        float startVol = lavaSource.volume;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // Usamos unscaled por si el juego está en pausa
            lavaSource.volume = Mathf.Lerp(startVol, target, timer / fadeDuration);
            yield return null;
        }
        lavaSource.volume = target;
    }
}