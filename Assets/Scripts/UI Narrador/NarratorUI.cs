using UnityEngine;
using UnityEngine.UI; // Necesario para el componente Image
using System.Collections;
using System.Collections.Generic;
using BitWave_Labs.AnimatedTextReveal;

public class NarratorUI : MonoBehaviour
{
    public static NarratorUI Instance;

    [Header("Referencias UI")]
    public RectTransform sidePanel;
    public AnimateText textAnimator;
    public Image narratorPortrait; // Arrastra aquí el objeto Image del panel

    [Header("Sprites")]
    public Sprite defaultNarratorSprite; // El sprite que se usa siempre por defecto

    [Header("Configuración de Movimiento")]
    public float xHidden = 950f; 
    public float xVisible = -177f; 
    public float slideSpeed = 10f;

    private bool isShowing = false;

    void Awake()
    {
        Instance = this;
        Vector2 pos = sidePanel.anchoredPosition;
        pos.x = xHidden;
        sidePanel.anchoredPosition = pos;

        // Establecer el sprite inicial
        if (narratorPortrait != null && defaultNarratorSprite != null)
            narratorPortrait.sprite = defaultNarratorSprite;
    }

    // Sobrecarga del método: Si no pasan sprite, usa null (que luego trataremos como el default)
    public void TriggerDialogue(List<string> lines, Sprite customSprite = null)
    {
        if (isShowing) return;
        StartCoroutine(ShowSequence(lines, customSprite));
    }

    private IEnumerator ShowSequence(List<string> lines, Sprite customSprite)
    {
        isShowing = true;

        // 0. Cambiar el sprite: si customSprite es null, usa el default
        if (narratorPortrait != null)
        {
            narratorPortrait.sprite = (customSprite != null) ? customSprite : defaultNarratorSprite;
        }

        // 1. Entrar
        yield return StartCoroutine(MoveToX(xVisible));

        // 2. Escribir texto
        yield return StartCoroutine(textAnimator.PlaySpecificLines(lines));

        // 3. Salir
        yield return StartCoroutine(MoveToX(xHidden));

        // 4. Volver al sprite por defecto para la próxima vez
        if (narratorPortrait != null)
            narratorPortrait.sprite = defaultNarratorSprite;

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