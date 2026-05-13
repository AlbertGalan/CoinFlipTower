using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class IntroSequenceManager : MonoBehaviour
{
    [Header("Referencias de Paneles")]
    public CanvasGroup panel1;
    public CanvasGroup panel2;

    [Header("Configuración de Tiempos (Panel 1)")]
    public float panel1FadeIn = 1.5f;
    public float panel1Display = 2.0f;
    public float panel1FadeOut = 1.0f;

    [Header("Configuración de Tiempos (Panel 2)")]
    public float panel2FadeIn = 2.0f; // Solicitado: 2 segundos
    public float panel2Display = 2.5f;

    [Header("Escena a Cargar")]
    public string sceneToLoad = "MenuPrincipal";

    private void Start()
    {
        // Aseguramos estado inicial: ambos invisibles
        if (panel1 != null) panel1.alpha = 0f;
        if (panel2 != null) panel2.alpha = 0f;

        // Iniciamos la secuencia
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        // --- SECUENCIA PANEL 1 ---
        if (panel1 != null)
        {
            // Fade In
            yield return StartCoroutine(FadeCanvasGroup(panel1, 0, 1, panel1FadeIn));
            
            // Tiempo de espera visible
            yield return new WaitForSeconds(panel1Display);
            
            // Fade Out
            yield return StartCoroutine(FadeCanvasGroup(panel1, 1, 0, panel1FadeOut));
        }

        yield return new WaitForSeconds(0.5f); // Pequeño respiro entre paneles

        // --- SECUENCIA PANEL 2 ---
        if (panel2 != null)
        {
            // Fade In (durante 2 segundos)
            yield return StartCoroutine(FadeCanvasGroup(panel2, 0, 1, panel2FadeIn));
            
            // Tiempo de espera visible
            yield return new WaitForSeconds(panel2Display);
        }

        // --- CARGAR ESCENA ---
        Debug.Log("Fin de Intro. Cargando MenuPrincipal...");
        LoadNextScene();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    private void LoadNextScene()
    {
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}