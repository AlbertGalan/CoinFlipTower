using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelToResultsTrigger : MonoBehaviour
{
    public string resultsSceneName = "Resultats";
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;

        // Guardem dades definitives per a la pantalla de resultats
        if (Score.Instance != null)
        {
            Score.Instance.SaveLastScore(); // Això fa servir el teu mètode original que guarda LastScore i LastTime
        }

        // Guardem la ruta del jugador
        PlayerRouteData routeData = other.GetComponent<PlayerRouteData>();
        if (routeData != null && routeData.tieneRutaAsignada)
        {
            PlayerPrefs.SetString("FinalRoute", routeData.rutaAsignada.ToString());
        }
        else
        {
            PlayerPrefs.SetString("FinalRoute", "Cap");
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene(resultsSceneName);
    }
}