using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Scene")]
    public string tutorialSceneName = "Tutorial";

    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    void Start()
    {
        // Mostrar cursor en el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Conectar botones
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnPlayClicked()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    void OnSettingsClicked()
    {
        Debug.Log("pulsado settings");
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
