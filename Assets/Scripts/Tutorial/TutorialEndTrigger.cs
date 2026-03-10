using UnityEngine;

/// <summary>
/// Trigger final del tutorial que guarda la puntuació, envia classificació i redirigeix.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialEndTrigger : MonoBehaviour
{
    [Header("Configuració")]
    [Tooltip("Nom de l'escena del menú principal")]
    public string menuSceneName = "MenuPrincipal";
    
    [Tooltip("Nom de l'escena de valoració")]
    public string ratingSceneName = "Valoracio";
    
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
        
        Debug.Log($"===== TUTORIAL END TRIGGER ACTIVATED =====");
        Debug.Log($"Triggered by: {other.name}");
        
        // Guardar puntuació final
        int finalScore = 0;

        if (Score.Instance != null)
        {
            Score.Instance.SaveLastScore();
            finalScore = Mathf.RoundToInt(Score.Instance.score);
            Debug.Log($"Score obtenido de Score.Instance: {finalScore}");
        }
        else
        {
            Debug.LogWarning("TutorialEndTrigger: No s'ha trobat Score.Instance a l'escena!");
            finalScore = Mathf.RoundToInt(Score.GetLastScore());
            Debug.Log($"Score obtenido de PlayerPrefs: {finalScore}");
        }

        // Fer POST de classificació i redirigir segons rated
        UserManager manager = UserManager.EnsureInstance();

        if (manager != null)
        {
            Debug.Log($"UserManager encontrado, enviando clasificación...");
            Debug.Log($"MenuScene: {menuSceneName}, RatingScene: {ratingSceneName}");
            
            manager.PostClassificationAndLoadNextScene(finalScore, menuSceneName, ratingSceneName);
        }
        else
        {
            Debug.LogWarning("TutorialEndTrigger: UserManager.Instance no disponible!");
            Debug.Log($"TutorialEndTrigger: fallback a menú ({menuSceneName})");
            UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
        }
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
