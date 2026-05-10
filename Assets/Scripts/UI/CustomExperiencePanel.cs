using UnityEngine;
using UnityEngine.UI;

public class CustomExperiencePanel : MonoBehaviour
{
    [Header("Configuración Tutorial")]
    [SerializeField] private Toggle tutorialToggle;

    [Header("Grupo de Rutas (Toggles)")]
    [SerializeField] private Toggle toggleTerra;
    [SerializeField] private Toggle toggleFoc;
    [SerializeField] private Toggle toggleGel;
    [SerializeField] private Toggle toggleAleatori;

    [Header("Botones de Control")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Navegación")]
    [SerializeField] private GameObject selectionPanel;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        backButton.onClick.AddListener(() => {
            selectionPanel.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    private void OnConfirmClicked()
    {
        Debug.Log("<color=cyan><b>[DEBUG]</b></color> Botón Confirmar pulsado en CustomExperiencePanel.");

        // 1. GESTIÓN DE RUTA
        PlayerPrefs.DeleteKey("SavedHasRoute");
        PlayerPrefs.DeleteKey("SavedRouteType");

        if (toggleAleatori.isOn)
        {
            PlayerPrefs.SetInt("SavedHasRoute", 0);
            Debug.Log("<color=yellow>Ruta elegida:</color> Aleatorio (No se guarda ruta fija).");
        }
        else
        {
            PlayerPrefs.SetInt("SavedHasRoute", 1);
            int rutaID = -1;

            if (toggleFoc.isOn) rutaID = (int)RouletteWheel.TipoElemento.Foc;
            else if (toggleGel.isOn) rutaID = (int)RouletteWheel.TipoElemento.Gel;
            else if (toggleTerra.isOn) rutaID = (int)RouletteWheel.TipoElemento.Terra;

            PlayerPrefs.SetInt("SavedRouteType", rutaID);
            Debug.Log($"<color=yellow>Ruta elegida:</color> ID {rutaID} (Foc=0, Gel=1, Terra=2 aproximadamente).");
        }

        // 2. COMPROBACIÓN DE USERMANAGER
        if (UserManager.Instance == null)
        {
            Debug.LogError("<color=red><b>[ERROR]</b></color> UserManager.Instance es NULL. La escena no cargará.");
            return;
        }

        // 3. GESTIÓN DE TUTORIAL Y ESCENA
        if (tutorialToggle.isOn)
        {
            Debug.Log("<color=green>Modo:</color> Tutorial Activo. Llamando a UserManager.StartGame() -> Cinematica.");
            UserManager.Instance.StartGame(); 
        }
        else
        {
            Debug.Log("<color=green>Modo:</color> Tutorial Saltado. Configurando Score=1000, Time=0.");
            
            PlayerPrefs.SetFloat("SavedScore", 1000f);
            PlayerPrefs.SetFloat("SavedTime", 0f);
            PlayerPrefs.SetInt("TutorialComplete", 1);
            PlayerPrefs.Save();

            Debug.Log("<color=orange>Llamando a UserManager.StartGame(\"PisNivell\")...</color>");
            UserManager.Instance.StartGame("PisNivell");
        }
    }
}