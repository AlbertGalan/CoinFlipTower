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

    [Header("Configuración de Movimiento")]
    [Tooltip("Posición X cuando el panel no se ve (fuera de pantalla)")]
    public float xHidden = 950f; 
    [Tooltip("Posición X cuando el panel está a la vista")]
    public float xVisible = -177f; 
    public float slideSpeed = 10f;

    private bool isShowing = false;

    void Awake()
    {
        Instance = this;
        // Colocamos el panel en su sitio inicial (oculto)
        Vector2 pos = sidePanel.anchoredPosition;
        pos.x = xHidden;
        sidePanel.anchoredPosition = pos;
    }

    public void TriggerDialogue(List<string> lines)
    {
        if (isShowing) return;
        StartCoroutine(ShowSequence(lines));
    }

    private IEnumerator ShowSequence(List<string> lines)
    {
        isShowing = true;

        // 1. Entrar (Derecha -> Izquierda)
        yield return StartCoroutine(MoveToX(xVisible));

        // 2. Escribir texto
        yield return StartCoroutine(textAnimator.PlaySpecificLines(lines));

        // 3. Salir (Izquierda -> Derecha)
        yield return StartCoroutine(MoveToX(xHidden));

        isShowing = false;
    }

    private IEnumerator MoveToX(float targetX)
    {
        Vector2 pos = sidePanel.anchoredPosition;
        while (Mathf.Abs(pos.x - targetX) > 0.1f)
        {
            pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * slideSpeed);
            sidePanel.anchoredPosition = pos;
            yield return null;
        }
        pos.x = targetX;
        sidePanel.anchoredPosition = pos;
    }
}