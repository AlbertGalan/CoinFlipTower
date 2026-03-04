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
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnAcceptClicked()
    {
        string name = nameInputField != null ? nameInputField.text : "";
        string email = emailInputField != null ? emailInputField.text : "";

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            Debug.LogWarning("Por favor, rellena todos los campos");
            return;
        }

        Debug.Log($"Credenciales ingresadas - Nombre: {name}, Email: {email}");

        // Llamar al UserManager para verificar y iniciar la partida
        UserManager.Instance.VerifyAndStartGame(name, email);

        // Cerrar el panel
        gameObject.SetActive(false);
    }

    private void OnCancelClicked()
    {
        Debug.Log("Credenciales canceladas");
        gameObject.SetActive(false);
    }
}
