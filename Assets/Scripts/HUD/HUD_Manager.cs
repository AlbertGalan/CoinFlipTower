using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI challengeText;
    
    [Header("Challenge Enter UI")]
    public GameObject challengeEnterPanel; // Panel contenedor para la intro
    public TextMeshProUGUI challengeEnterText; // Texto "CHALLENGE" con animación
    public TextMeshProUGUI challengeIdText; // Texto con ZoneID y tiempos
    public GameObject targetObject; // Objeto Target que se desactiva durante el challenge
    
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
    private bool updatingAnimator = false;

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
        
        if (challengeEnterPanel != null)
        {
            challengeEnterPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHUD();
        
        // Actualizar el Animator manualmente cuando Time.timeScale = 0
        if (updatingAnimator && challengeEnterText != null)
        {
            Animator animator = challengeEnterText.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Update(Time.unscaledDeltaTime);
            }
        }
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
        if (ChallengeTimerScore.activeZone != null && player != null)
        {
            float elapsed = ChallengeTimerScore.activeZone.GetElapsedTime(player);
            
            // Determinar color segons el temps transcorregut
            Color currentColor;
            if (elapsed <= ChallengeTimerScore.activeZone.goldTime)
            {
                currentColor = goldColor;
            }
            else if (elapsed <= ChallengeTimerScore.activeZone.silverTime)
            {
                currentColor = silverColor;
            }
            else
            {
                currentColor = bronzeColor;
            }
            
            challengeText.color = currentColor;
            challengeText.text = $"{ChallengeTimerScore.activeZone.zoneId} - {elapsed:F2}s";
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
    
    // Mètode per mostrar la introducció del challenge
    public void ShowChallengeEnter(string zoneId, float goldTime, float silverTime, float bronzeTime)
    {
        if (challengeEnterPanel == null) return;
        
        challengeEnterPanel.SetActive(true);
        updatingAnimator = true;
        
        if (challengeEnterText != null)
        {
            challengeEnterText.text = "CHALLENGE";
            
            // Activar la animación
            Animator animator = challengeEnterText.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("playChallenge");
            }
        }
        
        if (challengeIdText != null)
        {
            challengeIdText.text = $"{zoneId}\n{goldTime:F0}s - {silverTime:F0}s - {bronzeTime:F0}s";
        }
        
        // Desactivar el Target durante el challenge
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
    
    // Mètode per amagar la introducció del challenge
    public void HideChallengeEnter()
    {
        updatingAnimator = false;
        
        if (challengeEnterPanel != null)
        {
            challengeEnterPanel.SetActive(false);
        }
        
        // Reactivar el Target cuando se oculta el challenge
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }
}
