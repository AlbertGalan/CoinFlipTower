using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelToResultsTrigger : MonoBehaviour
{
    public string resultsSceneName = "Resultats";
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Solo actuamos si es el jugador y no se ha disparado ya
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        // 1. Guardamos datos definitivos para la pantalla de resultados
        if (Score.Instance != null)
        {
            Score.Instance.SaveLastScore(); 
        }

        // 2. Guardamos la ruta del jugador para mostrarla en resultados
        PlayerRouteData routeData = other.GetComponent<PlayerRouteData>();
        if (routeData != null && routeData.tieneRutaAsignada)
        {
            PlayerPrefs.SetString("FinalRoute", routeData.rutaAsignada.ToString());
        }
        else
        {
            PlayerPrefs.SetString("FinalRoute", "Cap");
        }

        // 3. Forzamos el guardado de PlayerPrefs en disco
        PlayerPrefs.Save();

        // 4. CAMBIO CLAVE: Usamos el LoadingManager para la transición
        if (LoadingManager.Instance != null)
        {
            Debug.Log("<color=green>Cargando resultados mediante LoadingManager...</color>");
            LoadingManager.Instance.LoadScene(resultsSceneName);
        }
        else
        {
            // Fallback por si el LoadingManager no existe en la escena
            Debug.LogWarning("LoadingManager no encontrado. Cargando de forma brusca.");
            SceneManager.LoadScene(resultsSceneName);
        }
    }
}