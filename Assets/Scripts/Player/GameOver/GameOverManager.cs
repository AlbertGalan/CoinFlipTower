using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameDisplayText;
    [SerializeField] private Button menuPrincipalButton;

private void Start()
{
    // --- RECUPERAR EL CURSOR ---
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None; 
    // ---------------------------

    // Tu lógica anterior del nombre...
    string savedName = PlayerPrefs.GetString("PlayerName", "Jugador");
    if (nameDisplayText != null) nameDisplayText.text = savedName;

    if (menuPrincipalButton != null) menuPrincipalButton.onClick.AddListener(GoToMenuPrincipal);
}
    public void GoToMenuPrincipal()
    {
        // Asegúrate de que el nombre de la escena sea exacto
        SceneManager.LoadScene("MenuPrincipal");
    }
}