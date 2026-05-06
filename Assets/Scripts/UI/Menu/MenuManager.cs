using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Scene")]
    public string tutorialSceneName = "Tutorial";

    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;
    public Button classificationButton;
    
    [Header("UI Panels")]
    public GameObject classificationPanel;
    public GameObject credentialsPanel;
    
    //[Header("Last Score Display")]
    //[Tooltip("TextMeshProUGUI per mostrar la darrera puntuació")]
    //public TextMeshProUGUI lastScoreText;
    
    //[Tooltip("TextMeshProUGUI per mostrar el darrer temps")]
    //public TextMeshProUGUI lastTimeText;

    void Start()
    {
        // Mostrar cursor en el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Conectar botones
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        
        if (classificationButton != null)
            classificationButton.onClick.AddListener(OnClassificationClicked);
        
        // Mostrar darrera puntuació si existeix
        //UpdateLastScoreDisplay();
    }
    
    /*void UpdateLastScoreDisplay()
    {
        float lastScore = Score.GetLastScore();
        string lastTime = Score.GetLastTimeFormatted();
        
        if (lastScoreText != null)
        {
            if (lastScore > 0f)
            {
                lastScoreText.text = $"Última Puntuació: {Mathf.RoundToInt(lastScore)}";
            }
            else
            {
                lastScoreText.text = "Última Puntuació: ---";
            }
        }
        
        if (lastTimeText != null)
        {
            if (lastScore > 0f)
            {
                lastTimeText.text = $"Temps: {lastTime}";
            }
            else
            {
                lastTimeText.text = "Temps: --:--:---";
            }
        }
    }*/

    void OnPlayClicked()
    {
        if (credentialsPanel != null)
        {
            credentialsPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("credentialsPanel no está asignado en el Inspector");
        }
    }

    void OnSettingsClicked()
    {
        Debug.Log("pulsado settings");
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    void OnClassificationClicked()
    {
        if (classificationPanel != null)
        {
            classificationPanel.SetActive(true);
        
        }
    }
}
