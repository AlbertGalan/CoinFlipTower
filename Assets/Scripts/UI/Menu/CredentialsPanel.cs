using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CredentialsPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;

    [Header("Siguiente Paso")]
    [SerializeField] private GameObject selectionPanel; // El panel con los 2 botones nuevos

    private void Start()
    {
        if (nameInputField != null) nameInputField.onValueChanged.AddListener(OnInputValueChanged);
        if (emailInputField != null) emailInputField.onValueChanged.AddListener(OnInputValueChanged);
        
        acceptButton.onClick.AddListener(OnAcceptClicked);
        cancelButton.onClick.AddListener(() => gameObject.SetActive(false));

        UpdateAcceptButtonState();
    }

private void OnAcceptClicked()
{
    string name = nameInputField.text;
    string email = emailInputField.text;
    acceptButton.interactable = false;

    UserManager.Instance.VerifyAndStartGame(name, email, (success) =>
    {
        if (success)
        {
            // 1. Si la referencia por Inspector se borró, la buscamos manualmente
            if (selectionPanel == null)
            {
                Debug.LogWarning("La referencia se perdió, buscándola por nombre...");
                // Asegúrate de que el objeto en la jerarquía se llame exactamente "Experiencia"
                selectionPanel = GameObject.Find("Experiencia"); 
            }

            // 2. Intentamos activar
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("No se pudo encontrar el panel 'Experiencia' en la escena.");
            }
        }
        else
        {
            acceptButton.interactable = true;
        }
    });
}
    private void OnInputValueChanged(string _) => UpdateAcceptButtonState();

    private void UpdateAcceptButtonState()
    {
        acceptButton.interactable = !string.IsNullOrWhiteSpace(nameInputField.text) && 
                                    !string.IsNullOrWhiteSpace(emailInputField.text);
    }
}