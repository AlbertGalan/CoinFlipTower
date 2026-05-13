using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class GameOverRouteTrigger : MonoBehaviour
{
    [Header("Configuración de Ruta")]
    [Tooltip("Lista de rutas que NO activarán el Game Over.")]
    public List<RouletteWheel.TipoElemento> rutasPermitidas;

    [Header("Configuración de Mensaje")]
    [Tooltip("El mensaje de tutorial que explica por qué ha muerto (opcional)")]
    public TutorialMessage deathMessage;
    public bool forceShowMessage = true;
    public bool applyPausaArtificial = true;

    [Header("Configuración de Escena")]
    public string gameOverSceneName = "GameOver";

    [Header("Filtros")]
    public List<string> triggerTags = new List<string> { "Player" };

    private bool isProcessStarted = false;

    private void OnEnable()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialMessageEnd.AddListener(OnMessageEnded);
        }
    }

    private void OnDisable()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialMessageEnd.RemoveListener(OnMessageEnded);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya estamos en proceso de Game Over, no repetir
        if (isProcessStarted) return;

        // Validar Tag
        if (!triggerTags.Contains(other.tag)) return;

        // Obtener datos de ruta del jugador
        PlayerRouteData routeData = other.GetComponent<PlayerRouteData>();
        if (routeData == null) return;

        // COMPROBACIÓN DE RUTA
        bool esRutaValida = routeData.tieneRutaAsignada && rutasPermitidas.Contains(routeData.rutaAsignada);

        if (esRutaValida)
        {
            // El jugador tiene permiso, no hacemos nada y dejamos que pase
            return;
        }
        else
        {
            // RUTA INCORRECTA: Iniciamos proceso de muerte
            isProcessStarted = true;
            StartGameOverSequence();
        }
    }

    private void StartGameOverSequence()
    {
        if (deathMessage != null && TutorialManager.Instance != null)
        {
            // Si hay mensaje, lo mostramos. El cambio de escena ocurrirá al cerrar el diálogo.
            TutorialManager.Instance.ShowMessage(deathMessage, forceShowMessage, applyPausaArtificial);
        }
        else
        {
            // Si no hay mensaje o manager, muerte instantánea
            ExecuteGameOver();
        }
    }

    private void OnMessageEnded()
    {
        // Solo si este trigger específico inició el proceso
        if (isProcessStarted)
        {
            ExecuteGameOver();
        }
    }

    private void ExecuteGameOver()
    {
        LoadingManager.Instance.LoadScene(gameOverSceneName);
    }

    private void OnDrawGizmos()
    {
        // Visualización en el editor
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // Rojo transparente
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
        }
    }
}