using BitWave_Labs.AnimatedTextReveal;
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
    public GameObject challengeEnterPanel; //Panel que conté el texte de "CHALLENGE" i ZoneID
    public TextMeshProUGUI challengeEnterText; // Texte "CHALLENGE" amb animació
    public TextMeshProUGUI challengeIdText; // Texte amb zoneId o id des challenge i es temps que te cada challenge

    private AnimatedTextReveal textAnimator;
    public GameObject targetObject; // Objecte del Target que se desactiva per veure es challenge
    private Coroutine challengeIdRoutine;
    
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

        if (challengeIdText != null)
        {
            textAnimator = challengeIdText.GetComponent<AnimatedTextReveal>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        UpdateHUD();
        
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
            string goldHex = ColorUtility.ToHtmlStringRGB(goldColor);
            string silverHex = ColorUtility.ToHtmlStringRGB(silverColor);
            string bronzeHex = ColorUtility.ToHtmlStringRGB(bronzeColor);

            challengeIdText.text =
                $"{zoneId}\n" +
                $"<color=#{goldHex}>{goldTime:F0}s</color> - " +
                $"<color=#{silverHex}>{silverTime:F0}s</color> - " +
                $"<color=#{bronzeHex}>{bronzeTime:F0}s</color>";
                
                if (textAnimator != null)
            {
                textAnimator.SetAllCharactersAlpha(0);
                if (challengeIdRoutine != null)
                {
                    StopCoroutine(challengeIdRoutine);
                }
                challengeIdRoutine = StartCoroutine(textAnimator.FadeText(true));
            }
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

        if (challengeIdRoutine != null)
        {
            StopCoroutine(challengeIdRoutine);
            challengeIdRoutine = null;
        }
        
        // Reactivar el Target cuando se oculta el challenge
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }
}
