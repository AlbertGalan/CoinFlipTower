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

    // --- GUARDAR EL NOMBRE ---
    PlayerPrefs.SetString("PlayerName", name);
    PlayerPrefs.Save(); // Asegura que se escriba en el disco
    // -------------------------

    acceptButton.interactable = false;

    UserManager.Instance.VerifyAndStartGame(name, email, (success) =>
    {
        if (success)
        {
            if (selectionPanel == null)
            {
                selectionPanel = GameObject.Find("Experiencia"); 
            }

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