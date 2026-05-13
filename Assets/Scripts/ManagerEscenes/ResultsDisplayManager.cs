using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultsDisplayManager : MonoBehaviour
{
    [Header("UI Textos")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    [Header("Escenarios de Ruta")]
    [Tooltip("Objeto visual para la ruta de Tierra")]
    public GameObject escenarioTerra;
    [Tooltip("Objeto visual para la ruta de Gel")]
    public GameObject escenarioGel;
    [Tooltip("Objeto visual para la ruta de Foc")]
    public GameObject escenarioFoc;

    [Header("Referencias UI")]
    public Button continueButton;

    [Header("Configuración Salida")]
    public string menuSceneName = "MenuPrincipal";
    public string ratingSceneName = "Valoracio";

    private int finalScore;
    private float finalTime;

    [Header("Audio")]
    public AudioSource resultsAudioSource; // El componente que emitirá el sonido
    public AudioClip resultsMusicClip;     // El archivo de música .mp3 o .wav

    [SerializeField] private TextMeshProUGUI nameDisplayText;


    void Start()
    {
        if(resultsAudioSource != null && resultsMusicClip != null)
        {
            resultsAudioSource.clip = resultsMusicClip;
            resultsAudioSource.loop = true; // Queremos que la música no pare
            resultsAudioSource.Play();
        }
        // 1. Recuperar datos de PlayerPrefs
        finalScore = Mathf.RoundToInt(PlayerPrefs.GetFloat("LastScore", 0));
        finalTime = PlayerPrefs.GetFloat("LastTime", 0);
        string formattedTime = PlayerPrefs.GetString("LastTimeFormatted", "00:00");
        
        // Recuperamos el String de la ruta ("Terra", "Gel" o "Foc")
        string routeName = PlayerPrefs.GetString("FinalRoute", "Cap");

        // 2. Mostrar datos básicos en pantalla
        if (scoreText != null) scoreText.text = "Puntuació: " + finalScore;
        if (timeText != null) timeText.text = "Temps: " + formattedTime;

        // 3. Lógica para mostrar el escenario correspondiente
        ActivarEscenarioRuta(routeName);

        // 4. Configurar el botón y cursor
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnClickContinuar);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        string savedName = PlayerPrefs.GetString("PlayerName", "Jugador");
        if (nameDisplayText != null) nameDisplayText.text = savedName;
    }

    private void ActivarEscenarioRuta(string ruta)
    {
        // Primero nos aseguramos de que todos estén apagados
        if (escenarioTerra != null) escenarioTerra.SetActive(false);
        if (escenarioGel != null) escenarioGel.SetActive(false);
        if (escenarioFoc != null) escenarioFoc.SetActive(false);

        // Activamos el que coincide con el nombre de la ruta guardada
        switch (ruta)
        {
            case "Terra":
                if (escenarioTerra != null) escenarioTerra.SetActive(true);
                Debug.Log("Mostrando escenario de Tierra");
                break;
            case "Gel":
                if (escenarioGel != null) escenarioGel.SetActive(true);
                Debug.Log("Mostrando escenario de Hielo");
                break;
            case "Foc":
                if (escenarioFoc != null) escenarioFoc.SetActive(true);
                Debug.Log("Mostrando escenario de Fuego");
                break;
            default:
                Debug.LogWarning("No se ha encontrado una ruta válida o el jugador no completó ninguna.");
                break;
        }
    }

    public void OnClickContinuar()
    {
        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.SaveFinalLog(finalScore, finalTime);
        }

        UserManager manager = UserManager.EnsureInstance();
        if (manager != null)
        {
            manager.PostClassificationAndLoadNextScene(finalScore, menuSceneName, ratingSceneName);
        }
        else
        {
            LoadingManager.Instance.LoadScene(ratingSceneName);
        }
    }
}