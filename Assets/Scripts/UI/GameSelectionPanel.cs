using UnityEngine;
using UnityEngine.UI;

public class GameSelectionPanel : MonoBehaviour
{
    [Header("Botón Flujo Normal")]
    [SerializeField] private Button startNormalFlowButton;

    [Header("Botón Abrir Tercer Panel")]
    [SerializeField] private Button openThirdPanelButton;
    [SerializeField] private GameObject thirdPanel; 

    private void Start()
    {
        // BOTÓN 1: Inicia el juego (Cinemática -> Tutorial)
        startNormalFlowButton.onClick.AddListener(OnStartNormalFlow);

        // BOTÓN 2: Abre el panel de Personalizada
        openThirdPanelButton.onClick.AddListener(OnOpenThirdPanel);
    }

    public void OnStartNormalFlow()
    {
        Debug.Log("<color=white><b>[FLUJO]</b></color> Iniciando Flujo Normal. Limpiando datos de ruta...");

        // --- LIMPIEZA DE DATOS DE RUTA ---
        // Borramos las claves que utiliza PlayerRouteData para que el jugador 
        // empiece el tutorial sin ninguna ruta pre-asignada.
        PlayerPrefs.DeleteKey("SavedHasRoute");
        PlayerPrefs.DeleteKey("SavedRouteType");
        
        // También es recomendable limpiar el estado del tutorial por si acaso
        PlayerPrefs.DeleteKey("TutorialComplete");
        
        // Guardamos los cambios en disco
        PlayerPrefs.Save();

        // Llamamos al UserManager para que cargue la escena inicial (Cinematica)
        if (UserManager.Instance != null)
        {
            UserManager.Instance.StartGame();
        }
    }

    private void OnOpenThirdPanel()
    {
        if (thirdPanel != null)
        {
            thirdPanel.SetActive(true);
            // Opcionalmente podrías desactivar este panel para que no se solapen
            // gameObject.SetActive(false); 
        }
    }
}