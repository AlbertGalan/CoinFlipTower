using UnityEngine;
using UnityEngine.SceneManagement; // Añadido para detectar la escena actual

public class Score : MonoBehaviour
{
    public static Score Instance { get; private set; }

    [Header("Variables de Estado")]
    public float score;
    public float timer;
    public bool tutorialComplete = false; // Nueva variable

    [Header("Configuración")]
    public float pointsPerSecond = 1f;
    public float gravityChangePenalty = 5f;

    private bool isGameplayFrozen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opcional: Si quieres que el objeto Score no se destruya nunca:
            // DontDestroyOnLoad(gameObject); 
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // --- CARGAR ESTADO DEL TUTORIAL ---
        // Usamos 0 para false y 1 para true
        tutorialComplete = PlayerPrefs.GetInt("TutorialComplete", 0) == 1;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "PisNivell")
        {
            score = PlayerPrefs.GetFloat("SavedScore", 1000f);
            timer = PlayerPrefs.GetFloat("SavedTime", 0f);
            Debug.Log($"<color=cyan>Score Inicializado:</color> Tutorial completado: {tutorialComplete}");
        }
        else
        {
            score = 1000f;
            timer = 0f;
        }
    }

    void Update()
    {
        if (isGameplayFrozen) return;
        timer += Time.deltaTime;
        score -= pointsPerSecond * Time.deltaTime;
    }

    // --- NUEVO MÉTODO PARA MARCAR COMPLETADO ---
    public void SetTutorialAsComplete()
    {
        tutorialComplete = true;
        PlayerPrefs.SetInt("TutorialComplete", 1);
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>Progreso Guardado:</color> Tutorial marcado como COMPLETADO.");
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
    // Añadimos estas:
    PlayerPrefs.DeleteKey("SavedHasRoute");
    PlayerPrefs.DeleteKey("SavedRouteType");
    PlayerPrefs.Save();
}
}