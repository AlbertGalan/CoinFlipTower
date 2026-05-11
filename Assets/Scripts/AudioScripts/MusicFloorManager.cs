using UnityEngine;
using System.Collections;

public class MusicFloorManager : MonoBehaviour
{
    public AudioSource sourceA;
    public AudioSource sourceB;
    
    private AudioSource activeSource;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Empezamos con el sourceA como activo pero en silencio
        activeSource = sourceA;
        sourceA.volume = 0;
        sourceB.volume = 0;
    }

    public void SwitchTrack(AudioClip newClip, float fadeTime)
    {
        // Si la canción que ya suena es la misma, no hacemos nada
        if (activeSource.isPlaying && activeSource.clip == newClip) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        
        // Intercambiamos los sources
        AudioSource newSource = (activeSource == sourceA) ? sourceB : sourceA;
        
        fadeCoroutine = StartCoroutine(Crossfade(newSource, newClip, fadeTime));
    }

    IEnumerator Crossfade(AudioSource newSource, AudioClip newClip, float fadeTime)
    {
        newSource.clip = newClip;
        newSource.Play();
        
        float timer = 0;
        float startVolumeActive = activeSource.volume;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            // El nuevo sube, el viejo baja
            newSource.volume = Mathf.Lerp(0, 1, t);
            activeSource.volume = Mathf.Lerp(startVolumeActive, 0, t);

            yield return null;
        }

        newSource.volume = 1;
        activeSource.volume = 0;
        activeSource.Stop();

        // El nuevo pasa a ser el activo para la próxima transición
        activeSource = newSource;
    }
}