using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger final del tutorial que guarda la puntuació i torna al menú principal
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialEndTrigger : MonoBehaviour
{
    [Header("Configuració")]
    [Tooltip("Nom de l'escena del menú principal")]
    public string menuSceneName = "MenuPrincipal";
    
    [Tooltip("Tags que activen el trigger (normalment 'Player')")]
    public string playerTag = "Player";
    
    [Header("Debug")]
    [Tooltip("Mostrar missatge de debug quan s'activa")]
    public bool showDebugMessages = true;
    
    private bool hasTriggered = false;
    
    private void Start()
    {
        // Assegurar que el collider és trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"TutorialEndTrigger '{gameObject.name}': El Collider no està marcat com Trigger!");
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Comprovar si ja s'ha activat
        if (hasTriggered)
        {
            return;
        }
        
        // Comprovar si el tag és correcte
        if (other.tag != playerTag)
        {
            return;
        }
        
        hasTriggered = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"TutorialEndTrigger '{gameObject.name}': Activat per '{other.name}', guardant puntuació i tornant al menú");
        }
        
        // Guardar puntuació final
        if (Score.Instance != null)
        {
            Score.Instance.SaveLastScore();
        }
        else
        {
            Debug.LogWarning("TutorialEndTrigger: No s'ha trobat Score.Instance a l'escena!");
        }
        
        // Tornar al menú
        SceneManager.LoadScene(menuSceneName);
    }
    
    private void OnDrawGizmos()
    {
        // Dibuixar el trigger en l'editor per facilitar la visualització
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = hasTriggered ? Color.gray : Color.green;
            
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
