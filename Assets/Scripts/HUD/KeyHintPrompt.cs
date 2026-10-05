using TMPro;
using UnityEngine;

public class KeyHintPrompt : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel o GameObject que contiene el mensaje")]
    public GameObject promptRoot;

    [Tooltip("Texto del mensaje (TextMeshProUGUI)")]
    public TextMeshProUGUI promptText;

    [Header("Configuración")]
    public KeyCode keyToPress = KeyCode.E;

    [TextArea(2, 4)]
    public string message = "Pulsa E para avanzar un diálogo";

    private void Start()
    {
        if (promptText != null)
        {
            promptText.text = message;
        }

        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            if (promptRoot != null)
            {
                promptRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}