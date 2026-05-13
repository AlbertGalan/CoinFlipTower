using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Necesario para Corrutinas

[RequireComponent(typeof(Collider))]
public class GameOverRouteTrigger : MonoBehaviour
{
    [Header("Configuración de Ruta")]
    public List<RouletteWheel.TipoElemento> rutasPermitidas;

    [Header("Configuración de Mensaje")]
    public TutorialMessage deathMessage;
    public bool forceShowMessage = true;
    public bool applyPausaArtificial = true;

    [Header("Configuración de Escena")]
    public string gameOverSceneName = "GameOver";

    [Header("Filtros")]
    public List<string> triggerTags = new List<string> { "Player" };

    private bool isProcessStarted = false;
    private bool hasListenerRegistered = false;

    private void OnEnable()
    {
        // No registramos el listener en OnEnable; lo hacemos solo cuando se activa el trigger
        hasListenerRegistered = false;
    }

    private void OnDisable()
    {
        RemoveMessageListener();
    }

    private void RegisterMessageListener()
    {
        if (hasListenerRegistered) return; // Evitar duplicados
        if (TutorialManager.Instance == null) return;

        TutorialManager.Instance.OnTutorialMessageEnd.AddListener(OnMessageEnded);
        hasListenerRegistered = true;
        Debug.Log($"<color=cyan>[GameOverRouteTrigger]</color> Listener registrado en {gameObject.name}");
    }

    private void RemoveMessageListener()
    {
        if (!hasListenerRegistered) return;
        if (TutorialManager.Instance == null) return;

        TutorialManager.Instance.OnTutorialMessageEnd.RemoveListener(OnMessageEnded);
        hasListenerRegistered = false;
        Debug.Log($"<color=cyan>[GameOverRouteTrigger]</color> Listener removido de {gameObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isProcessStarted) return;
        if (!triggerTags.Contains(other.tag)) return;

        PlayerRouteData routeData = other.GetComponent<PlayerRouteData>();
        if (routeData == null) return;

        bool esRutaValida = routeData.tieneRutaAsignada && rutasPermitidas.Contains(routeData.rutaAsignada);

        Debug.Log($"<color=orange>[GameOverRouteTrigger]</color> Trigger activado en {gameObject.name}. " +
                  $"Ruta del jugador: {(routeData.tieneRutaAsignada ? routeData.rutaAsignada.ToString() : "NINGUNA")}, " +
                  $"Rutas permitidas: {string.Join(", ", rutasPermitidas)}, " +
                  $"¿Es válida?: {esRutaValida}");

        if (!esRutaValida)
        {
            isProcessStarted = true;
            Debug.Log($"<color=red>[GameOverRouteTrigger]</color> Ruta incorrecta detectada en {gameObject.name}");
            StartGameOverSequence();
        }
    }

    private void StartGameOverSequence()
    {
        Debug.Log($"<color=yellow>[GameOverRouteTrigger]</color> Iniciando secuencia de game over en {gameObject.name}");
        
        if (deathMessage != null && TutorialManager.Instance != null)
        {
            RegisterMessageListener();
            Debug.Log($"<color=yellow>[GameOverRouteTrigger]</color> Mostrando mensaje de muerte. hasListenerRegistered={hasListenerRegistered}");
            TutorialManager.Instance.ShowMessage(deathMessage, forceShowMessage, applyPausaArtificial);
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[GameOverRouteTrigger]</color> No hay mensaje o TutorialManager. Ejecutando game over directo.");
            ExecuteGameOver();
        }
    }

    private void OnMessageEnded()
    {
        Debug.Log($"<color=cyan>[GameOverRouteTrigger]</color> OnMessageEnded llamado. isProcessStarted={isProcessStarted}, hasListenerRegistered={hasListenerRegistered}, activeObject={gameObject.name}");
        
        // Si este trigger activó el proceso, esperamos un suspiro y cargamos escena
        if (isProcessStarted)
        {
            Debug.Log("<color=red>[GameOverRouteTrigger]</color> El mensaje ha terminado. Iniciando carga de escena...");
            RemoveMessageListener();
            StartCoroutine(WaitAndLoad());
        }
        else
        {
            Debug.LogWarning("<color=yellow>[GameOverRouteTrigger]</color> OnMessageEnded llamado pero isProcessStarted es false - posible listener de otro trigger");
        }
    }

    private IEnumerator WaitAndLoad()
    {
        // Esperamos un frame o un tiempo mínimo para que el TutorialManager 
        // termine de procesar su cierre completamente (limpieza de UI, etc.)
        yield return new WaitForSecondsRealtime(0.1f);
        ExecuteGameOver();
    }

    private void ExecuteGameOver()
    {
        Debug.Log($"<color=red>[GameOverRouteTrigger]</color> Ejecutando game over. Cargando escena: {gameOverSceneName}");
        
        if (LoadingManager.Instance != null)
        {
            Debug.Log($"<color=red>[GameOverRouteTrigger]</color> Usando LoadingManager para cargar {gameOverSceneName}");
            LoadingManager.Instance.LoadScene(gameOverSceneName);
        }
        else
        {
            // Fallback por si el LoadingManager no está en la escena
            Debug.LogWarning($"<color=red>[GameOverRouteTrigger]</color> LoadingManager no encontrado. Usando SceneManager directo.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
        }
    }

    // Dibujado de Gizmos se mantiene igual...
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
        }
    }
}