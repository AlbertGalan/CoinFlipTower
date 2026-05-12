using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameOverAfterTutorial : MonoBehaviour
{
    [Header("Configuración de Zona")]
    public List<RouletteWheel.TipoElemento> rutasPermitidas;
    public string gameOverSceneName = "GameOver";

    private bool waitingForGameOver = false;
    private TutorialTrigger tutorialTrigger;

    private void Awake()
    {
        // Buscamos si hay un tutorial trigger en este mismo objeto
        tutorialTrigger = GetComponent<TutorialTrigger>();
    }

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
                bool esRutaValida = routeData.tieneRutaAsignada && rutasPermitidas.Contains(routeData.rutaAsignada);

                if (!esRutaValida)
                {
                    // COMPROBACIÓN CRÍTICA
                    // Si no hay tutorial trigger, o el tutorial ya se mostró (hasTriggered),
                    // o no hay manager... matamos directamente.
                    if (tutorialTrigger == null || (tutorialTrigger.triggerOnce && !PuedeMostrarTutorial()))
                    {
                        SceneManager.LoadScene(gameOverSceneName);
                    }
                    else
                    {
                        // Si el tutorial SÍ se va a mostrar, esperamos al evento
                        waitingForGameOver = true;
                    }
                }
            }
        }
    }

    private bool PuedeMostrarTutorial()
    {
        // Si el TutorialTrigger ya se activó una vez, ya no volverá a disparar el evento de fin
        // Por lo tanto, no podemos quedarnos esperando.
        // Accedemos mediante reflexión o simplemente asumiendo que si está desactivado no actuará.
        // Como 'hasTriggered' es privado en tu script, usaremos una lógica de seguridad:
        return tutorialTrigger.enabled; 
    }

    private void CheckIfShouldDie()
    {
        if (waitingForGameOver)
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}