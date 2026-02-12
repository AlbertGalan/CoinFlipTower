using TMPro;
using UnityEngine;

public class ClassificationRowUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    private void OnEnable()
    {
        // Si no están asignados manualmente, buscar automáticamente en los hijos
        if (nameText == null || scoreText == null)
        {
            TextMeshProUGUI[] textComponents = GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponents.Length >= 2)
            {
                nameText = textComponents[0];
                scoreText = textComponents[1];
                Debug.Log("Componentes TextMeshPro encontrados automáticamente en: " + gameObject.name);
            }
            else
            {
                Debug.LogError("No se encontraron suficientes componentes TextMeshProUGUI en: " + gameObject.name);
            }
        }
    }

    public void SetData(int position, string name, int score)
    {
        if (nameText != null)
            nameText.text = name;
        else
            Debug.LogError("nameText es NULL en " + gameObject.name);
        
        if (scoreText != null)
            scoreText.text = score.ToString();
        else
            Debug.LogError("scoreText es NULL en " + gameObject.name);
    }

    public void SetHeaderStyle(Color textColor)
    {
        if (nameText != null)
            nameText.color = textColor;
        else
            Debug.LogError("nameText es NULL en " + gameObject.name);

        if (scoreText != null)
            scoreText.color = textColor;
        else
            Debug.LogError("scoreText es NULL en " + gameObject.name);
    }

    public void SetContentFontSize(float fontSize)
    {
        if (nameText != null)
            nameText.fontSize = fontSize;
        else
            Debug.LogError("nameText es NULL en " + gameObject.name);

        if (scoreText != null)
            scoreText.fontSize = fontSize;
        else
            Debug.LogError("scoreText es NULL en " + gameObject.name);
    }
}
