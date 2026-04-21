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

    [Header("Efectos y Salida")]
    public GameObject lavaWaterfall;
    public UnityEvent OnSolved;
    public UnityEvent OnWrongAttempt;

    private bool isChecking = false;

    void Awake()
    {
        InitializeBulbs();
        UpdateVisualBar();
    }

    private void InitializeBulbs()
    {
        if (bulbContainer == null) return;
        
        // Limpiamos la lista por si acaso
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
        Debug.Log("Bombillas inicializadas: " + bulbLights.Count);
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
            // Si la presión es 5, se encienden los índices 0,1,2,3,4
            bulbLights[i].enabled = (i < currentPressure);
            bulbLights[i].color = colorNormal; 
        }

        if (counterText != null)
        {
            counterText.text = currentPressure.ToString();
            // Pista visual: si al restar 1 llegamos al objetivo, se pone Cyan
            counterText.color = (currentPressure - 1 == targetPressure) ? Color.cyan : Color.white;
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
        
        // Si tenemos 5 bombillas (índices 0 a 4), la que se resta es la 4.
        int bulbToSubtractIndex = currentPressure - 1; 
        int resultPressure = currentPressure - 1;

        // 1. EFECTO DE RESTA: Solo la última bombilla cambia a colorResta (Rojo)
        if (bulbToSubtractIndex >= 0 && bulbToSubtractIndex < bulbLights.Count)
        {
            bulbLights[bulbToSubtractIndex].color = colorResta;
            // Actualizamos el texto para mostrar el valor "resultante" durante la espera
            if (counterText != null) counterText.text = resultPressure.ToString();
        }

        yield return new WaitForSeconds(0.6f);

        // Apagamos físicamente esa bombilla
        if (bulbToSubtractIndex >= 0 && bulbToSubtractIndex < bulbLights.Count)
            bulbLights[bulbToSubtractIndex].enabled = false;

        yield return new WaitForSeconds(0.4f);

        // 2. COMPROBACIÓN
        if (resultPressure == targetPressure)
        {
            // ÉXITO: Todas las bombillas que quedan encendidas se ponen verdes
            SetAllActiveBulbsColor(colorCorrecto, resultPressure);
            if (counterText != null) counterText.color = colorCorrecto;
            Solve();
        }
        else
        {
            // ERROR: Todas las que quedan encendidas se ponen rojas
            SetAllActiveBulbsColor(colorError, resultPressure);
            if (counterText != null) counterText.color = colorError;
            
            OnWrongAttempt?.Invoke();

            yield return new WaitForSeconds(1.2f);

            // Restaurar estado anterior (vuelven a color normal y se re-enciende la restada)
            currentPressure = initialValue;
            UpdateVisualBar();
            isChecking = false;
        }
    }

    private void SetAllActiveBulbsColor(Color targetColor, int count)
    {
        // 'count' es la cantidad de bombillas que deben estar encendidas
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