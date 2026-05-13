using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using TMPro;

public class PressurePuzzleManager : MonoBehaviour
{
    [Header("Configuración del Puzzle")]
    public int targetPressure = 14; 
    public int maxPressure = 20;
    public int currentPressure = 0;

    [Header("Colores")]
    public Color colorNormal = Color.white;
    public Color colorCorrecto = Color.green;
    public Color colorError = Color.red;
    public Color colorResta = Color.red; 

    [Header("Referencias Visuales")]
    public Transform bulbContainer;
    private List<Light> bulbLights = new List<Light>();

    [Tooltip("Texto de TextMeshPro para el contador numérico")]
    public TextMeshProUGUI counterText; 

    [Header("Audio")]
    [Tooltip("Fuente de audio que reproducirá los sonidos")]
    public AudioSource audioSource;
    [Tooltip("Sonido cuando el puzzle se resuelve correctamente")]
    public AudioClip solveClip;
    [Tooltip("Sonido cuando el resultado es incorrecto")]
    public AudioClip wrongClip;

    [Header("Efectos y Salida")]
    public GameObject lavaWaterfall;
    public UnityEvent OnSolved;
    public UnityEvent OnWrongAttempt;

    private bool isChecking = false;

    void Awake()
    {
        InitializeBulbs();
        UpdateVisualBar();

        // Intento automático de obtener el AudioSource si no se asignó
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void InitializeBulbs()
    {
        if (bulbContainer == null) return;
        bulbLights.Clear();

        foreach (Transform child in bulbContainer)
        {
            Light l = child.GetComponentInChildren<Light>();
            if (l != null)
            {
                bulbLights.Add(l);
                l.enabled = false;
            }
        }
    }

    public void AddPressure(int amount)
    {
        if (isChecking) return;
        currentPressure = Mathf.Clamp(currentPressure + amount, 0, maxPressure);
        UpdateVisualBar();
    }

    private void UpdateVisualBar()
    {
        for (int i = 0; i < bulbLights.Count; i++)
        {
            bulbLights[i].enabled = (i < currentPressure);
            bulbLights[i].color = colorNormal; 
        }

        if (counterText != null)
        {
            counterText.text = currentPressure.ToString();
            counterText.color = colorNormal;
        }
    }

    public void TryActivate()
    {
        if (!isChecking && currentPressure > 0) StartCoroutine(ActivationSequence());
    }

    private IEnumerator ActivationSequence()
    {
        isChecking = true;
        int initialValue = currentPressure;
        int bulbToSubtractIndex = currentPressure - 1; 
        int resultPressure = currentPressure - 1;

        // 1. EFECTO DE RESTA
        if (bulbToSubtractIndex >= 0 && bulbToSubtractIndex < bulbLights.Count)
        {
            bulbLights[bulbToSubtractIndex].color = colorResta;
            if (counterText != null) 
            {
                counterText.text = resultPressure.ToString();
                counterText.color = colorResta;
            }
        }

        yield return new WaitForSeconds(0.6f);

        if (bulbToSubtractIndex >= 0 && bulbToSubtractIndex < bulbLights.Count)
            bulbLights[bulbToSubtractIndex].enabled = false;

        yield return new WaitForSeconds(0.4f);

        // 2. COMPROBACIÓN
        if (resultPressure == targetPressure)
        {
            // --- ÉXITO ---
            PlaySound(solveClip);
            if (counterText != null) counterText.color = colorCorrecto;
            SetAllActiveBulbsColor(colorCorrecto, resultPressure);
            Solve();
        }
        else
        {
            // --- FALLO ---
            PlaySound(wrongClip);
            if (counterText != null) counterText.color = colorError;
            SetAllActiveBulbsColor(colorError, resultPressure);
            
            OnWrongAttempt?.Invoke();

            yield return new WaitForSeconds(1.2f);

            // 3. RESTAURACIÓN
            currentPressure = initialValue;
            UpdateVisualBar(); 
            isChecking = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SetAllActiveBulbsColor(Color targetColor, int count)
    {
        for (int i = 0; i < bulbLights.Count; i++)
        {
            if (i < count)
            {
                bulbLights[i].enabled = true;
                bulbLights[i].color = targetColor;
            }
            else
            {
                bulbLights[i].enabled = false;
            }
        }
    }

    private void Solve()
    {
        if (lavaWaterfall != null) lavaWaterfall.SetActive(false);
        OnSolved?.Invoke();
    }
}