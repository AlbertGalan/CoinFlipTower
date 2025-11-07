using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    
    private Score scoreScript;

    [System.Obsolete]
    void Start()
    {
        //Cercam Script que manetja la puntuació
        scoreScript = FindObjectOfType<Score>();

        Color c = timeText.color;
        c.a = 0.5f; // Establir l'opacitat a 50%
        timeText.color = c;
        
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
    }
}
