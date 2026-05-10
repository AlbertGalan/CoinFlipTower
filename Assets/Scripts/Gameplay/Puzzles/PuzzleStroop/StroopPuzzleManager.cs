using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class StroopPuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public struct StroopData {
        public string name;
        public Color color;
    }

    [Header("Configuración Colores")]
    public List<StroopData> colorPool; 

    [Header("Referencias UI Central")]
    public TextMeshProUGUI mainIndicatorText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;
    public RectTransform indicatorCanvas; 

    [Header("Casillas")]
    public List<StroopChoice> choices;

    [Header("Lógica de Juego")]
    public int totalRounds = 10;
    public float roundTime = 5f;
    public UnityEvent onPuzzleComplete;

    private int currentRound = 0;
    private int correctAnswers = 0;
    private float currentTime;
    private bool isGameActive = false;
    private string targetColorName; 

    public bool IsGameActive => isGameActive; 
    public bool IsPuzzleFullySolved => correctAnswers >= 5 && !isGameActive;

    private void Start()
    {
        // Al iniciar, ponemos los nombres correspondientes en las casillas
        InitializeChoices();
    }

    private void InitializeChoices()
    {
        // Emparejamos cada casilla con un color del pool por orden
        for (int i = 0; i < choices.Count; i++)
        {
            if (i < colorPool.Count)
            {
                choices[i].colorName = colorPool[i].name;
                if (choices[i].textUI != null)
                {
                    choices[i].textUI.text = colorPool[i].name;
                    choices[i].textUI.color = Color.black; // Texto de casilla siempre legible
                }
            }
        }
    }

    public void StartPuzzle()
    {
        if (isGameActive || IsPuzzleFullySolved) return; 
        currentRound = 0;
        correctAnswers = 0;
        isGameActive = true;
        StartCoroutine(NextRound());
    }

IEnumerator NextRound()
{
    if (currentRound < totalRounds)
    {
        currentRound++;
        UpdateRoundUI();

        // --- NUEVA LÓGICA DE DIFICULTAD ---
        if (currentRound <= 5)
        {
            // Rondas 1 a 5: Orden original siempre
            InitializeChoices();
        }
        else
        {
            // Ronda 6 en adelante: Cambiar nombres de sitio cada vez
            ShuffleChoices();
        }
        // ----------------------------------

        SetupChallenge();
        
        currentTime = roundTime;
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            timerText.text = currentTime.ToString("F1") + "s";
            yield return null;
        }

        ProcessAnswer(""); 
    }
    else
    {
        EndPuzzle();
    }
}
private void ShuffleChoices()
{
    // Creamos una lista temporal con los datos del pool de colores
    List<StroopData> shuffledPool = new List<StroopData>(colorPool);

    // Algoritmo de barajado Fisher-Yates
    for (int i = 0; i < shuffledPool.Count; i++)
    {
        StroopData temp = shuffledPool[i];
        int randomIndex = Random.Range(i, shuffledPool.Count);
        shuffledPool[i] = shuffledPool[randomIndex];
        shuffledPool[randomIndex] = temp;
    }

    // Asignamos los nuevos nombres a las casillas
    for (int i = 0; i < choices.Count; i++)
    {
        if (i < shuffledPool.Count)
        {
            choices[i].colorName = shuffledPool[i].name;
            if (choices[i].textUI != null)
            {
                choices[i].textUI.text = shuffledPool[i].name;
            }
        }
    }
    Debug.Log("<color=orange>Casillas barajadas!</color>");
}
void SetupChallenge()
{
    // 1. Elegir palabra e índice de color aleatorios
    int wordIdx = Random.Range(0, colorPool.Count);
    int colorIdx = Random.Range(0, colorPool.Count);

    // TEST DE STROOP REAL:
    // Forzamos que el color visual sea distinto a la palabra escrita 
    // para que el cerebro tenga que esforzarse en ignorar la lectura.
    if (colorIdx == wordIdx)
    {
        colorIdx = (colorIdx + 1) % colorPool.Count;
    }

    // 2. Aplicar la PALABRA (lo que se lee)
    mainIndicatorText.text = colorPool[wordIdx].name;
    mainIndicatorText.enableVertexGradient = false;

    // 3. Aplicar el COLOR VISUAL (lo que el jugador debe pulsar)
    Color indicatorColor = colorPool[colorIdx].color;
    indicatorColor.a = 1f; // Aseguramos que sea opaco
    mainIndicatorText.color = indicatorColor;
    
    // El objetivo SIEMPRE es el nombre del color que vemos
    targetColorName = colorPool[colorIdx].name;

    // Forzar actualización visual de TextMeshPro
    mainIndicatorText.SetAllDirty(); 

    // 4. Lógica de Rotación (Dificultad progresiva)
    // De la ronda 8 a la 10, el texto aparecerá girado
    if (currentRound >= 8) 
    {
        indicatorCanvas.localEulerAngles = new Vector3(0, 0, Random.Range(-180, 180));
    } 
    else 
    {
        indicatorCanvas.localEulerAngles = Vector3.zero;
    }

    // Debug para que veas en consola qué color espera el script
    Debug.Log($"Ronda {currentRound}: Palabra '{colorPool[wordIdx].name}' pintada de {targetColorName}");
}

    public void OnPlayerClick(StroopChoice choice)
    {
        if (!isGameActive) return;
        StopAllCoroutines();
        ProcessAnswer(choice.colorName);
    }

    void ProcessAnswer(string answer)
    {
        bool correct = (answer == targetColorName);
        if (correct) correctAnswers++;

        foreach (var c in choices) 
        {
            c.SetVisualFeedback(correct);
        }

        StartCoroutine(WaitAndNext());
    }

    IEnumerator WaitAndNext()
    {
        yield return new WaitForSeconds(0.6f);
        StartCoroutine(NextRound());
    }

void EndPuzzle()
{
    isGameActive = false;
    timerText.text = "0.0s";
    indicatorCanvas.localEulerAngles = Vector3.zero;
    mainIndicatorText.enableVertexGradient = false;

    if (correctAnswers >= 5)
    {
        // Determinamos el título según los aciertos
        if (correctAnswers == 10) 
            mainIndicatorText.text = "MESTRE DEL STROOP";
        else if (correctAnswers >= 7) 
            mainIndicatorText.text = "EXPERT DEL STROOP";
        else 
            mainIndicatorText.text = "APRENENT DEL STROOP";

        mainIndicatorText.color = Color.green; 
        onPuzzleComplete.Invoke();
        AddFinalPoints();
    }
    else
    {
        mainIndicatorText.text = "REINTENTAR";
        mainIndicatorText.color = Color.red; 
    }
}

    void AddFinalPoints()
    {
        if (Score.Instance == null) return;
        if (correctAnswers == 10) Score.Instance.AddPoints(150f, "Mestre del Stroop");
        else if (correctAnswers >= 7) Score.Instance.AddPoints(100f, "Expert del Stroop");
        else if (correctAnswers >= 5) Score.Instance.AddPoints(50f, "Aprenent del Stroop");
    }

    void UpdateRoundUI()
    {
        roundText.text = currentRound + "/" + totalRounds;
    }
}