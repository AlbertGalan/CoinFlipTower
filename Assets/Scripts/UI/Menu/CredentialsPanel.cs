using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CredentialsPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;

    private void Start()
    {
        if (nameInputField != null)
            nameInputField.onValueChanged.AddListener(OnInputValueChanged);

        if (emailInputField != null)
            emailInputField.onValueChanged.AddListener(OnInputValueChanged);

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        UpdateAcceptButtonState();
    }

    private void OnEnable()
    {
        UpdateAcceptButtonState();
    }

    private void OnAcceptClicked()
    {
        string name = nameInputField != null ? nameInputField.text : "";
        string email = emailInputField != null ? emailInputField.text : "";

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            Debug.LogWarning("Per favor, ompleix les credencials abans de continuar.");
            return;
        }

        Debug.Log($"Credencials ingresades - Nom: {name}, Email: {email}");

        // Llamar al UserManager para verificar y iniciar la partida
        if (UserManager.Instance == null)
        {
            Debug.LogError("UserManager.Instance no está disponible");
            return;
        }

        // Deshabilitar el botón mientras se procesa
        if (acceptButton != null)
            acceptButton.interactable = false;

        // Llamar con callback - solo cerrar si hay error
        UserManager.Instance.VerifyAndStartGame(name, email, (success) =>
        {
            if (!success)
            {
                // Si falla, cerrar el panel y rehabilitar botón
                if (acceptButton != null)
                    acceptButton.interactable = true;
                    
                gameObject.SetActive(false);
            }
            // Si tiene éxito, la escena cambiará automáticamente (no cerrar el panel)
        });
    }

    private void OnInputValueChanged(string _)
    {
        UpdateAcceptButtonState();
    }

    private void UpdateAcceptButtonState()
    {
        if (acceptButton == null)
            return;

        string name = nameInputField != null ? nameInputField.text : "";
        string email = emailInputField != null ? emailInputField.text : "";

        acceptButton.interactable = !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email);
    }

    private void OnCancelClicked()
    {
        Debug.Log("Credencials cancelades");
        gameObject.SetActive(false);
    }
}
