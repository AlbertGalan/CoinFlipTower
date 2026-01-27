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
        TutorialManager.Instance.ShowMessage(tutorialMessage, forceShow);
        
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
