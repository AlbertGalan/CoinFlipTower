using UnityEngine;

public class Score : MonoBehaviour
{
    public static Score Instance { get; private set; }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    //Variable puntuació
    public float score;
    //Timer en minuts, segons i mil·lesimes de segon, Formatar!! (JA ESTA FORMATAT)
    public float timer;


    public float pointsPerSecond = 1f;
    public float gravityChangePenalty = 5f;

    //Referenciam scripts de gravetat per poder detectar quan un objecte o quan el jugador canvia de gravetat i restar-li puntuació en funció.
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

        score = 1000f;
        timer = 0f;

    }

    // Update is called once per frame
    void Update()
    {

        timer += Time.deltaTime;

        score -= pointsPerSecond * Time.deltaTime;

        Debug.Log("Temps:" + " " + timer + " " + "Puntuació:" + score);


    }

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
    
    // Mètode per sumar punts des de zones o altres sistemes
    public void AddPoints(float points, string source = "")
    {
        score += points;
        if (!string.IsNullOrEmpty(source))
        {
            Debug.Log($"Punts afegits: +{points} ({source}). Puntuació total: {score:F0}");
        }
    }
    
    /// <summary>
    /// Guarda la puntuació i temps finals a PlayerPrefs
    /// </summary>
    public void SaveLastScore()
    {
        PlayerPrefs.SetFloat("LastScore", score);
        PlayerPrefs.SetFloat("LastTime", timer);
        PlayerPrefs.SetString("LastTimeFormatted", GetFormattedTime());
        PlayerPrefs.Save();
        Debug.Log($"Puntuació final guardada: {score:F0} - Temps: {GetFormattedTime()}");
    }
    
    /// <summary>
    /// Obté la darrera puntuació guardada
    /// </summary>
    public static float GetLastScore()
    {
        return PlayerPrefs.GetFloat("LastScore", 0f);
    }
    
    /// <summary>
    /// Obté el darrer temps guardat (formatat)
    /// </summary>
    public static string GetLastTimeFormatted()
    {
        try
        {
            return PlayerPrefs.GetString("LastTimeFormatted", "00:00.000");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error obtenint temps formatat: {e.Message}");
            return "00:00.000";
        }
    }
}
