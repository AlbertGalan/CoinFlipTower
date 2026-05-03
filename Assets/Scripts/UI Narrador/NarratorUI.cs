using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BitWave_Labs.AnimatedTextReveal;

public class NarratorUI : MonoBehaviour
{
    public static NarratorUI Instance;

    [Header("Referencias")]
    public RectTransform sidePanel;
    public AnimateText textAnimator;

    [Header("Configuración de Posiciones (Eje X)")]
    // Basado en tus datos de Left/Right
    public float hiddenLeft = 1394f;
    public float hiddenRight = -1394f;
    
    public float visibleLeft = 668f;
    public float visibleRight = -668f;

    public float slideSpeed = 8f; // Velocidad del deslizamiento

    private bool isShowing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Inicializamos el panel en posición oculta
        SetPanelOffsets(hiddenLeft, hiddenRight);
    }

    public void TriggerDialogue(List<string> lines)
    {
        if (isShowing) return;
        StartCoroutine(ShowSequence(lines));
    }

    private IEnumerator ShowSequence(List<string> lines)
    {
        isShowing = true;

        // 1. Deslizar hacia adentro (Hacia 668, -668)
        yield return StartCoroutine(MovePanel(visibleLeft, visibleRight));

        // 2. Reproducir el texto
        // Llamamos a la función que añadimos antes a tu script AnimateText
        yield return StartCoroutine(textAnimator.PlaySpecificLines(lines));

        // 3. Deslizar hacia afuera (Hacia 1394, -1394)
        yield return StartCoroutine(MovePanel(hiddenLeft, hiddenRight));

        isShowing = false;
    }

    private IEnumerator MovePanel(float targetLeft, float targetRight)
    {
        float currentLeft = sidePanel.offsetMin.x;
        float currentRight = sidePanel.offsetMax.x;

        float elapsed = 0;
        while (Mathf.Abs(currentLeft - targetLeft) > 0.1f)
        {
            currentLeft = Mathf.Lerp(currentLeft, targetLeft, Time.deltaTime * slideSpeed);
            currentRight = Mathf.Lerp(currentRight, targetRight, Time.deltaTime * slideSpeed);
            
            SetPanelOffsets(currentLeft, currentRight);
            yield return null;
        }

        SetPanelOffsets(targetLeft, targetRight);
    }

    private void SetPanelOffsets(float left, float right)
    {
        // offsetMin.x es el 'Left' en el Inspector
        // offsetMax.x es el 'Right' en el Inspector (Unity lo guarda negativo si es hacia la derecha)
        sidePanel.offsetMin = new Vector2(left, sidePanel.offsetMin.y);
        sidePanel.offsetMax = new Vector2(right, sidePanel.offsetMax.y);
    }
}