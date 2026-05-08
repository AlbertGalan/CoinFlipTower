using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pausePanel;
    public bool allowPauseWithEsc = true;
    public bool lockCursorOnResume = true;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (allowPauseWithEsc && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);

            // --- AQUÍ VA LA MEJORA ---
            // Buscamos el componente PlayerRouteData para actualizar la imagen de la ruta
            PlayerRouteData routeData = Object.FindFirstObjectByType<PlayerRouteData>();
            if (routeData != null)
            {
                routeData.ActualizarImagenRutaUI();
            }
            // -------------------------
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (lockCursorOnResume)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ExitMainMenuScene()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }
}