using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

public class RatingSceneManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private StarRatingGroup generalGroup;
    [SerializeField] private StarRatingGroup jugabilitatGroup;
    [SerializeField] private StarRatingGroup dificultatGroup;
    [SerializeField] private StarRatingGroup graficsGroup;
    [SerializeField] private StarRatingGroup concordanciaGroup;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button submitButton;

    [Header("Configuración")]
    [SerializeField] private NetworkingData networkingData;
    [SerializeField] private string mainMenuSceneName = "MenuPrincipal";

    [Header("Feedback UI (opcional)")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMPro.TextMeshProUGUI feedbackText;

    private const int MinRating = 1;
    private const int MaxRating = 5;

    private StarRatingGroup[] activeRatingGroups;
    private bool isSubmitting = false;
    private CanvasGroup loadingCanvasGroup;

    private void Awake()
    {
        EnsureEventSystem();
        BindButtons();
    }

    void Start()
    {
        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Cargar NetworkingData si no está asignado
        if (networkingData == null)
            networkingData = Resources.Load<NetworkingData>("NetworkingData");

        CacheRatingGroups();
        SubscribeToRatingChanges();
        UpdateSubmitButtonState();

        // Ocultar panel de carga si existe
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
            loadingCanvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.interactable = false;
                loadingCanvasGroup.blocksRaycasts = false;
            }
        }

        ValidateReferences();
    }

    private void OnDestroy()
    {
        UnsubscribeFromRatingChanges();
    }

    /// <summary>
    /// Botón Salir - Vuelve al menú principal sin enviar valoración
    /// </summary>
    private void OnExitClicked()
    {
        Debug.Log("Saliendo sin valorar");
        LoadMainMenu();
    }

    /// <summary>
    /// Botón Aceptar - Envía la valoración y vuelve al menú principal
    /// </summary>
    private void OnSubmitClicked()
    {
        if (isSubmitting)
            return;

        if (!AreAllRatingsValid())
        {
            ShowFeedback("Completa todas las valoraciones (1-5)");
            UpdateSubmitButtonState();
            return;
        }

        UserManager manager = UserManager.EnsureInstance();
        if (manager == null)
        {
            Debug.LogError("UserManager.Instance no está disponible");
            ShowFeedback("Error: Usuario no encontrado");
            return;
        }

        // Iniciar el envío de la valoración
        StartCoroutine(SubmitRatingCoroutine(manager));
    }

    /// <summary>
    /// Envía la valoración a la API
    /// </summary>
    private IEnumerator SubmitRatingCoroutine(UserManager manager)
    {
        isSubmitting = true;
        UpdateSubmitButtonState();

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.interactable = true;
                loadingCanvasGroup.blocksRaycasts = true;
            }
        }

        ShowFeedback("Enviando valoración...");

        // Validar configuració
        if (networkingData == null)
        {
            Debug.LogError("NetworkingData no configurat");
            ShowFeedback("Error de configuració");
            isSubmitting = false;
            yield return new WaitForSeconds(2f);
            LoadMainMenu();
            yield break;
        }

        // Obtenir dades d'usuari per enviar amb la valoració
        if (!manager.TryGetCurrentCredentials(out string userName, out string userEmail))
        {
            Debug.LogError("Dades d'usuari no disponibles per enviar la valoració");
            ShowFeedback("Error: No s'han trobat les dades d'usuari");
            isSubmitting = false;
            yield return new WaitForSeconds(2f);
            LoadMainMenu();
            yield break;
        }

        Debug.Log($"Enviant rateGame amb usuari: {userName} ({userEmail})");

        RateGameDTO rateGameData = new RateGameDTO
        {
            api_token = networkingData.ApiToken,
            name = userName,
            email = userEmail,
            general = generalGroup.currentRating,
            jugabilitat = jugabilitatGroup.currentRating,
            dificultat = dificultatGroup.currentRating,
            grafics = graficsGroup.currentRating,
            concordancia = concordanciaGroup.currentRating
        };

        string json = JsonConvert.SerializeObject(rateGameData);
        string url = $"{networkingData.ApiUrl}/rateGame";

        Debug.Log($"POST rating a {url} para usuario {userName}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Valoración enviada correctamente");
                Debug.Log($"Respuesta: {request.downloadHandler.text}");
                ShowFeedback("¡Gracias por tu valoración!");
                
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                Debug.LogError($"Error al enviar valoración: {request.error} - Código: {request.responseCode}");
                Debug.LogError($"Respuesta: {request.downloadHandler.text}");
                ShowFeedback("Error al enviar valoración");
                
                yield return new WaitForSeconds(2f);
            }
        }

        isSubmitting = false;
        LoadMainMenu();
    }

    private void CacheRatingGroups()
    {
        activeRatingGroups = new[]
        {
            generalGroup,
            jugabilitatGroup,
            dificultatGroup,
            graficsGroup,
            concordanciaGroup
        };
    }

    private void SubscribeToRatingChanges()
    {
        if (activeRatingGroups == null)
            return;

        for (int i = 0; i < activeRatingGroups.Length; i++)
        {
            if (activeRatingGroups[i] != null)
                activeRatingGroups[i].OnRatingChanged += OnAnyRatingChanged;
        }
    }

    private void UnsubscribeFromRatingChanges()
    {
        if (activeRatingGroups == null)
            return;

        for (int i = 0; i < activeRatingGroups.Length; i++)
        {
            if (activeRatingGroups[i] != null)
                activeRatingGroups[i].OnRatingChanged -= OnAnyRatingChanged;
        }
    }

    private void OnAnyRatingChanged(int _)
    {
        UpdateSubmitButtonState();
    }

    private void UpdateSubmitButtonState()
    {
        if (submitButton == null)
            return;

        submitButton.interactable = !isSubmitting && AreAllRatingsValid();
    }

    private bool AreAllRatingsValid()
    {
        if (activeRatingGroups == null || activeRatingGroups.Length != 5)
            return false;

        for (int i = 0; i < activeRatingGroups.Length; i++)
        {
            StarRatingGroup group = activeRatingGroups[i];

            if (group == null)
                return false;

            int rating = group.currentRating;
            if (rating < MinRating || rating > MaxRating)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Muestra un mensaje de feedback al usuario
    /// </summary>
    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            Debug.Log($"Feedback: {message}");
        }
    }

    /// <summary>
    /// Carga la escena del menú principal
    /// </summary>
    private void LoadMainMenu()
    {
        Debug.Log($"Cargando escena: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void BindButtons()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitClicked);
            exitButton.onClick.AddListener(OnExitClicked);
        }

        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitClicked);
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
        Debug.LogWarning("No había EventSystem en la escena. Se ha creado automáticamente.");
    }

    private void ValidateReferences()
    {
        if (exitButton == null)
            Debug.LogError("RatingSceneManager: exitButton no asignado en Inspector.");

        if (submitButton == null)
            Debug.LogError("RatingSceneManager: submitButton no asignado en Inspector.");

        if (generalGroup == null || jugabilitatGroup == null || dificultatGroup == null || graficsGroup == null || concordanciaGroup == null)
            Debug.LogError("RatingSceneManager: faltan uno o más StarRatingGroup en Inspector.");
    }
}
