using UnityEngine;
using UnityEngine.SceneManagement; // Añadido para detectar la escena actual

public class Score : MonoBehaviour
{
    public static Score Instance { get; private set; }

    [Header("Variables de Estado")]
    public float score;
    public float timer;

    [Header("Configuración")]
    public float pointsPerSecond = 1f;
    public float gravityChangePenalty = 5f;

    private bool isGameplayFrozen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Detectamos en qué escena estamos
        string currentScene = SceneManager.GetActiveScene().name;

        // Si estamos en el nivel principal, intentamos recuperar la puntuación del tutorial
        if (currentScene == "PisNivell")
        {
            // Cargamos los datos guardados en el trigger del tutorial. 
            // Si no existen (por si acaso), usamos los valores base de 1000 y 0.
            score = PlayerPrefs.GetFloat("SavedScore", 1000f);
            timer = PlayerPrefs.GetFloat("SavedTime", 0f);
            
            Debug.Log($"<color=cyan>Score Inicializado en PisNivell:</color> Continuando con Score: {score:F0} y Tiempo: {timer:F2}");
        }
        else
        {
            // Si es el tutorial o cualquier otra escena, empezamos desde los valores base
            score = 1000f;
            timer = 0f;
            
            Debug.Log("<color=green>Score Inicializado:</color> Valores de Tutorial cargados (1000 pts / 0s)");
        }
    }

    void Update()
    {
        // No actualizar si el gameplay está congelado por tutoriales
        if (isGameplayFrozen)
            return;

        timer += Time.deltaTime;
        score -= pointsPerSecond * Time.deltaTime;
    }

    // --- Métodos de Control ---

    public void FreezeGameplay()
    {
        isGameplayFrozen = true;
    }

    public void UnfreezeGameplay()
    {
        isGameplayFrozen = false;
    }

    public bool IsGameplayFrozen => isGameplayFrozen;

    // --- Métodos de Obtención de Datos ---

    public string GetFormattedTime()
    {
        int minutes = (int)(timer / 60f);
        int seconds = (int)(timer % 60f);
        int milliseconds = (int)((timer * 1000f) % 1000f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    public int GetScoreInt()
    {
        return Mathf.RoundToInt(score);
    }

    public float GetTimer()
    {
        return timer;
    }

    public void AddPoints(float points, string source = "")
    {
        score += points;
        if (!string.IsNullOrEmpty(source))
        {
            Debug.Log($"Punts afegits: +{points} ({source}). Puntuació total: {score:F0}");
        }
    }

    // --- Métodos de Persistencia ---

    /// <summary>
    /// Guarda la puntuación y tiempo finales a PlayerPrefs para la escena de Resultados
    /// </summary>
    public void SaveLastScore()
    {
        PlayerPrefs.SetFloat("LastScore", score);
        PlayerPrefs.SetFloat("LastTime", timer);
        PlayerPrefs.SetString("LastTimeFormatted", GetFormattedTime());
        PlayerPrefs.Save();
        Debug.Log($"Puntuació final guardada: {score:F0} - Temps: {GetFormattedTime()}");
    }

    public static float GetLastScore()
    {
        return PlayerPrefs.GetFloat("LastScore", 0f);
    }

    public static string GetLastTimeFormatted()
    {
        return PlayerPrefs.GetString("LastTimeFormatted", "00:00.000");
    }
    
    /// <summary>
    /// Limpia las claves temporales (útil al volver al menú principal)
    /// </summary>
    public static void ClearTransitionData()
    {
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.DeleteKey("SavedTime");
        PlayerPrefs.Save();
    }
}