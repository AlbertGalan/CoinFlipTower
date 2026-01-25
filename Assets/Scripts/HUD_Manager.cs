using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI challengeText;
    
    private Score scoreScript;
    private GameObject player;
    
    [Header("Colors del repte")]
    public Color goldColor = new Color(1f, 0.84f, 0f); // Daurat
    public Color silverColor = new Color(0.75f, 0.75f, 0.75f); // Platejat
    public Color bronzeColor = new Color(0.8f, 0.5f, 0.2f); // Bronze
    
    private float challengeCompleteTimer = 0f;
    private bool showingCompletion = false;
    private int lastPointsAwarded = 0;
    private string lastRank = "";

    [System.Obsolete]
    void Start()
    {
        //Cercam Script que manetja la puntuació
        scoreScript = FindObjectOfType<Score>();
        player = GameObject.FindGameObjectWithTag("Player");

        Color c = timeText.color;
        c.a = 0.5f; // Establir l'opacitat a 50%
        timeText.color = c;
        
        if (challengeText != null)
        {
            challengeText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (scoreScript != null)
        {
            timeText.text = "TIME - " + scoreScript.GetFormattedTime();
            scoreText.text = "SCORE - " + scoreScript.GetScoreInt().ToString();
        }
        
        UpdateChallengeText();
    }
    
    void UpdateChallengeText()
    {
        if (challengeText == null) return;
        
        // Mostrar missatge de compleció
        if (showingCompletion)
        {
            challengeCompleteTimer -= Time.deltaTime;
            if (challengeCompleteTimer <= 0f)
            {
                showingCompletion = false;
                challengeText.gameObject.SetActive(false);
            }
            return;
        }
        
        // Comprovar si hi ha una zona activa
        if (ZoneTimerScoring.activeZone != null && player != null)
        {
            float elapsed = ZoneTimerScoring.activeZone.GetElapsedTime(player);
            
            // Determinar color segons el temps transcorregut
            Color currentColor;
            if (elapsed <= ZoneTimerScoring.activeZone.goldTime)
            {
                currentColor = goldColor;
            }
            else if (elapsed <= ZoneTimerScoring.activeZone.silverTime)
            {
                currentColor = silverColor;
            }
            else
            {
                currentColor = bronzeColor;
            }
            
            challengeText.color = currentColor;
            challengeText.text = $"{ZoneTimerScoring.activeZone.zoneId} - {elapsed:F2}s";
            challengeText.gameObject.SetActive(true);
        }
        else if (challengeText.gameObject.activeSelf && !showingCompletion)
        {
            challengeText.gameObject.SetActive(false);
        }
    }
    
    // Mètode públic per mostrar els punts guanyats
    public void ShowChallengeComplete(int points, string rank)
    {
        if (challengeText == null) return;
        
        lastPointsAwarded = points;
        lastRank = rank;
        showingCompletion = true;
        challengeCompleteTimer = 4f; // Mostrar durant 4 segons
        
        Color rankColor = rank == "OR" ? goldColor : (rank == "PLATA" ? silverColor : bronzeColor);
        challengeText.color = rankColor;
        challengeText.text = $"+{points}";
        challengeText.gameObject.SetActive(true);
    }
}
