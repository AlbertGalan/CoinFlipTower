using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    // Singleton: Para poder llamarlo desde cualquier script sin arrastrar referencias
    public static LoadingManager Instance;

    [Header("Referencias UI")]
    [SerializeField] private GameObject loadingPanel; // El Panel hijo del Canvas
    [SerializeField] private Slider progressBar;       // El Slider
    [SerializeField] private TextMeshProUGUI progressText; // El texto de %

    private void Awake()
    {
        // Lógica de Singleton: Si ya existe uno, borra este. Si no, haz que este sea el oficial.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // <--- ESTO LO HACE GLOBAL
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // El método que llamaremos para cambiar de escena
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

private IEnumerator LoadAsync(string sceneName)
{
    // 1. Activa el panel inmediatamente
    loadingPanel.SetActive(true);
    
    // 2. ¡CRUCIAL! Espera un frame o dos. 
    // Esto obliga a Unity a renderizar la pantalla de carga antes de empezar a procesar la escena pesada.
    yield return new WaitForSeconds(0.1f); 

    // 3. Ahora iniciamos la carga
    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
    
    // Para evitar que Unity se coma todo el CPU y congele la animación de la barra:
    operation.allowSceneActivation = false; 

    while (!operation.isDone)
    {
        float progress = Mathf.Clamp01(operation.progress / 0.9f);
        if (progressBar != null) progressBar.value = progress;
        if (progressText != null) progressText.text = (progress * 100f).ToString("F0") + "%";

        // Si la carga llega al 90% (que es el 100% real para Unity)
        if (operation.progress >= 0.9f)
        {
            // Opcional: Un pequeño retraso para que el usuario vea el 100%
            if (progressText != null) progressText.text = "100%";
            yield return new WaitForSeconds(0.5f);
            
            // Activamos la escena
            operation.allowSceneActivation = true;
        }

        yield return null;
    }

    loadingPanel.SetActive(false);
}
}