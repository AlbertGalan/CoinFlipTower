using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Trigger que activa un missatge del tutorial quan el jugador entra.
/// Col·loca aquest component en un GameObject amb Collider (marcat com Trigger).
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Configuració del trigger")]
    [Tooltip("Missatge del tutorial que s'activarà")]
    public TutorialMessage tutorialMessage;
    
    [Tooltip("Tags que activen el trigger (normalment 'Player')")]
    public List<string> triggerTags = new List<string> { "Player" };
    
    [Tooltip("Activar només una vegada")]
    public bool triggerOnce = true;
    
    [Tooltip("Forçar mostrar el missatge encara que ja s'hagi mostrat abans")]
    public bool forceShow = false;

    [Header("Pausa artificial (opcional)")]
    [Tooltip("Sobrescriure la configuració de pausa artificial per aquest trigger")]
    public bool overridePausaArtificial = false;
    
    [Tooltip("Aplicar pausa artificial mentre es mostra aquest missatge (s'aplica només si overridePausaArtificial és true)")]
    public bool pausaArtificialDuringMessage = true;
    
    [Header("Destrucció")]
    [Tooltip("Destruir aquest GameObject després d'activar-se")]
    public bool destroyAfterTrigger = false;
    
    [Tooltip("Temps d'espera abans de destruir (segons)")]
    public float destroyDelay = 0f;
    
    [Header("Debug")]
    [Tooltip("Mostrar missatge de debug quan s'activa")]
    public bool showDebugMessages = false;
    
    [Header("Esdeveniments")]
    public UnityEvent OnTriggerActivated;
    
    private bool hasTriggered = false;
    
    private void Awake()
    {
        // Subscriure's a l'esdeveniment de final de missatge del TutorialManager
        // Això permet que aquest trigger es comprovi quan un altre missatge acabi
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialMessageEnd.AddListener(OnTutorialMessageEnded);
        }
    }
    
    private void OnDestroy()
    {
        // Desubscriure's de l'esdeveniment
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialMessageEnd.RemoveListener(OnTutorialMessageEnded);
        }
    }
    
    private void Start()
    {
        // Validar que tenim el missatge assignat
        if (tutorialMessage == null)
        {
            Debug.LogWarning($"TutorialTrigger '{gameObject.name}': No té TutorialMessage assignat!");
        }
        
        // Assegurar que el collider és trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"TutorialTrigger '{gameObject.name}': El Collider no està marcat com Trigger!");
            col.isTrigger = true;
        }
        
        // Esperar a que la escena esté completamente cargada
        StartCoroutine(CheckPlayerAfterSceneLoaded());
    }
    
    private System.Collections.IEnumerator CheckPlayerAfterSceneLoaded()
    {
        // Esperar un frame a que todo esté inicializado
        yield return null;
        CheckIfPlayerAlreadyInside();
    }
    
    /// <summary>
    /// Cridat quan un missatge del tutorial acaba.
    /// Comprova si el jugador ja està dins del trigger per activar-lo.
    /// </summary>
    private void OnTutorialMessageEnded()
    {
        // Si aquest trigger ja s'ha activat, no fer res
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        // Esperar un frame perquè el TutorialManager acabi de netejar
        StartCoroutine(CheckPlayerAfterMessageEnded());
    }
    
    private System.Collections.IEnumerator CheckPlayerAfterMessageEnded()
    {
        yield return null;
        CheckIfPlayerAlreadyInside();
    }
    
    private void CheckIfPlayerAlreadyInside()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        // Usar OverlapBox para detectar si hay un objeto con los tags dentro
        Collider[] overlappingColliders = Physics.OverlapBox(
            col.bounds.center,
            col.bounds.extents,
            transform.rotation
        );
        
        foreach (Collider other in overlappingColliders)
        {
            // Ignorar nuestro propio collider
            if (other == col) continue;
            
            // Comprobar si este collider tiene uno de los tags requeridos
            if (triggerTags.Contains(other.tag))
            {
                if (showDebugMessages)
                {
                    Debug.Log($"TutorialTrigger '{gameObject.name}': Jugador '{other.name}' ja detectat dins. Activant trigger.");
                }
                
                // Activar el trigger como si acabara de entrar
                OnTriggerEnter(other);
                return;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Comprovar si ja s'ha activat
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        // Comprovar si el tag és correcte
        if (!triggerTags.Contains(other.tag))
        {
            return;
        }
        
        // Comprovar si TutorialManager està mostrant un mensaje
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsShowingMessage())
        {
            if (showDebugMessages)
            {
                Debug.Log($"TutorialTrigger '{gameObject.name}': Ignorat perquè el TutorialManager ja està mostrant un missatge.");
            }
            return;
        }
        
        // Comprovar si tenim missatge i manager
        if (tutorialMessage == null)
        {
            Debug.LogError($"TutorialTrigger '{gameObject.name}': No pot activar-se sense TutorialMessage!");
            return;
        }
        
        if (TutorialManager.Instance == null)
        {
            Debug.LogError($"TutorialTrigger '{gameObject.name}': No hi ha TutorialManager a l'escena!");
            return;
        }
        
        // Marcar com activat
        hasTriggered = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"TutorialTrigger '{gameObject.name}': Activat per '{other.name}', mostrant missatge '{tutorialMessage.messageId}'");
        }
        
        // Activar el missatge
        if (overridePausaArtificial)
        {
            TutorialManager.Instance.ShowMessage(tutorialMessage, forceShow, pausaArtificialDuringMessage);
        }
        else
        {
            TutorialManager.Instance.ShowMessage(tutorialMessage, forceShow);
        }
        
        // Disparar esdeveniment
        OnTriggerActivated?.Invoke();
        
        // Destruir si està configurat
        if (destroyAfterTrigger)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
    
    /// <summary>
    /// Reinicia el trigger perquè es pugui activar de nou.
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    private void OnDrawGizmos()
    {
        // Dibuixar el trigger en l'editor per facilitar la visualització
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = hasTriggered ? Color.gray : Color.yellow;
            
            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
        }
    }
}
