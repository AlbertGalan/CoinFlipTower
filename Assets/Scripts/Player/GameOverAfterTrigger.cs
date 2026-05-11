using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Esto es necesario para usar el método .Contains()

public class GameOverAfterTutorial : MonoBehaviour
{
    [Header("Configuración de Zona")]
    [Tooltip("Añade aquí los elementos del Enum que SÍ pueden pasar")]
    // Esto crea una lista de tu Enum en el Inspector
    public List<RouletteWheel.TipoElemento> rutasPermitidas;
    
    public string gameOverSceneName = "GameOver";

    private bool waitingForGameOver = false;

    private void OnEnable()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialMessageEnd.AddListener(CheckIfShouldDie);
        }
    }

    private void OnDisable()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialMessageEnd.RemoveListener(CheckIfShouldDie);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRouteData routeData = other.GetComponent<PlayerRouteData>();

            if (routeData != null)
            {
                // Comprobamos si la ruta actual del jugador (que es un Enum)
                // está dentro de nuestra lista de Enums permitidos.
                bool esRutaValida = routeData.tieneRutaAsignada && rutasPermitidas.Contains(routeData.rutaAsignada);

                if (!esRutaValida)
                {
                    waitingForGameOver = true;
                    // El TutorialTrigger hará el resto
                }
            }
        }
    }

    private void CheckIfShouldDie()
    {
        if (waitingForGameOver)
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}