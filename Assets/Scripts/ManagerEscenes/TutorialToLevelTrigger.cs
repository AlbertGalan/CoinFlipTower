using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialToLevelTrigger : MonoBehaviour
{
    public string nextSceneName = "PisNivell";
    public string playerTag = "Player";
    private bool hasTriggered = false;

private void OnTriggerEnter(Collider other)
{
    if (hasTriggered || !other.CompareTag(playerTag)) return;
    hasTriggered = true;

    // 1. Guardar datos de Score (lo que ya teníamos)
    if (Score.Instance != null)
    {
        PlayerPrefs.SetFloat("SavedScore", Score.Instance.score);
        PlayerPrefs.SetFloat("SavedTime", Score.Instance.timer);
        Score.Instance.SetTutorialAsComplete();
    }

    // 2. NUEVO: Guardar datos de la Ruta del Jugador
    PlayerRouteData playerRoute = other.GetComponent<PlayerRouteData>();
    if (playerRoute != null)
    {
        // Guardamos si tiene ruta (0 o 1)
        PlayerPrefs.SetInt("SavedHasRoute", playerRoute.tieneRutaAsignada ? 1 : 0);
        // Guardamos el Enum (se guarda como un número entero automáticamente)
        PlayerPrefs.SetInt("SavedRouteType", (int)playerRoute.rutaAsignada);
        
        Debug.Log($"<color=orange>Persistencia:</color> Guardada ruta {playerRoute.rutaAsignada}");
    }

    PlayerPrefs.Save();
    //SceneManager.LoadScene(nextSceneName);
    LoadingManager.Instance.LoadScene(nextSceneName);
}
}