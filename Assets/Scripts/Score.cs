using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Score : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    //Variable puntuació
    public float score;
    //Timer en minuts, segons i mil·lesimes de segon, Formatar!! (JA ESTA FORMATAT)
    public float timer;


    public float pointsPerSecond = 1f;
    public float gravityChangePenalty = 5f;

    //Referenciam scripts de gravetat per poder detectar quan un objecte o quan el jugador canvia de gravetat i restar-li puntuació en funció.
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
}
