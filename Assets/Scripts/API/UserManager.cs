using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class UserManager : MonoBehaviour
{
    // Singleton
    public static UserManager Instance { get; private set; }

    [SerializeField] private NetworkingData networkingData;

    // Dades usuari actuals
    private string currentUserName;
    private string currentUserEmail;

    // Respuesta del verify
    private VerifyResponseDTO verifyResponse;
    private bool isPostingClassification;

    private const string UserNameKey = "SessionUserName";
    private const string UserEmailKey = "SessionUserEmail";
    private const string UserRatedKey = "SessionUserRated";

    public static UserManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        UserManager existing = FindFirstObjectByType<UserManager>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("UserManager_Auto");
        return go.AddComponent<UserManager>();
    }

    private void Awake()
    {
        // Implementar Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureNetworkingData();
        LoadSessionFromPrefs();
    }

    private void Start()
    {
        EnsureNetworkingData();
    }

    /// <summary>
    /// Verifica el usuario con la API y, si es exitoso, inicia la partida
    /// </summary>
    public void VerifyAndStartGame(string userName, string userEmail, System.Action<bool> onComplete = null)
    {
        currentUserName = userName;
        currentUserEmail = userEmail;
        SaveSessionToPrefs();
        StartCoroutine(VerifyUserCoroutine(onComplete));
    }

    private IEnumerator VerifyUserCoroutine(System.Action<bool> onComplete = null)
    {
        if (!EnsureNetworkingData())
        {
            Debug.LogError("NetworkingData no configurado");
            onComplete?.Invoke(false);
            yield break;
        }

        // Crea l'objecte de la request
        var verifyRequest = new
        {
            api_token = networkingData.ApiToken,
            name = currentUserName,
            email = currentUserEmail
        };

        string json = JsonConvert.SerializeObject(verifyRequest);
        string url = $"{networkingData.ApiUrl}/verify";

        Debug.Log($"POST a {url} amb dades: {json}");

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
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Resposta del verify: " + jsonResponse);

                verifyResponse = JsonConvert.DeserializeObject<VerifyResponseDTO>(jsonResponse);

                if (verifyResponse != null)
                {
                    Debug.Log($"===== VERIFY RESPONSE =====");
                    Debug.Log($"Usuari verificat: {currentUserName} ({currentUserEmail})");
                    Debug.Log($"Rated: {verifyResponse.rated} (tipo: {verifyResponse.rated.GetType()})");
                    Debug.Log($"Criteris disponibles: {verifyResponse.criterion.Count}");

                    foreach (var criterion in verifyResponse.criterion)
                    {
                        Debug.Log($"  - {criterion.name} ({criterion.min_score}-{criterion.max_score})");
                    }
                    Debug.Log($"===========================");

                    SaveSessionToPrefs();

                    // Iniciar la partida
                    onComplete?.Invoke(true);
                    StartGame();
                }
                else
                {
                    Debug.LogError("No s'ha pogut deserialitzar la resposta del verify");
                    onComplete?.Invoke(false);
                }
            }
            else
            {
                Debug.LogError($"Error en verify: {request.error} - Codi: {request.responseCode}");
                Debug.LogError($"Resposta: {request.downloadHandler.text}");
                onComplete?.Invoke(false);
            }
        }
    }

    private void StartGame()
    {
        Debug.Log("Iniciando partida...");
        SceneManager.LoadScene("Tutorial");
    }

    /// <summary>
    /// Publica la puntuació a la classificació i carrega la següent escena segons si l'usuari ha valorat o no
    /// </summary>
    public void PostClassificationAndLoadNextScene(int score, string menuSceneName, string ratingSceneName)
    {
        if (isPostingClassification)
            return;

        StartCoroutine(PostClassificationCoroutine(score, menuSceneName, ratingSceneName));
    }

    private IEnumerator PostClassificationCoroutine(int score, string menuSceneName, string ratingSceneName)
    {
        Debug.Log($"===== POST CLASSIFICATION STARTED =====");
        Debug.Log($"Score: {score}");
        Debug.Log($"User: {currentUserName} ({currentUserEmail})");
        
        isPostingClassification = true;

        if (!EnsureNetworkingData())
        {
            Debug.LogError("NetworkingData no configurat per a classification POST");
            LoadSceneByRated(menuSceneName, ratingSceneName);
            isPostingClassification = false;
            yield break;
        }

        if (string.IsNullOrWhiteSpace(currentUserName) || string.IsNullOrWhiteSpace(currentUserEmail))
        {
            Debug.LogError("No hi ha credencials d'usuari vàlides per a classification POST");
            LoadSceneByRated(menuSceneName, ratingSceneName);
            isPostingClassification = false;
            yield break;
        }

        PostScoreDTO postData = new PostScoreDTO
        {
            api_token = networkingData.ApiToken,
            name = currentUserName,
            email = currentUserEmail,
            puntuacion = score
        };

        string json = JsonConvert.SerializeObject(postData);
        string url = $"{networkingData.ApiUrl}/classification";
        
        Debug.Log($"POST URL: {url}");
        Debug.Log($"POST JSON: {json}");

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
                Debug.Log("===== CLASSIFICATION POST SUCCESS =====");
                Debug.Log($"Response code: {request.responseCode}");
                Debug.Log($"Response body: {request.downloadHandler.text}");
                Debug.Log($"=======================================");
            }
            else
            {
                Debug.LogError("===== CLASSIFICATION POST ERROR =====");
                Debug.LogError($"Error: {request.error}");
                Debug.LogError($"Response code: {request.responseCode}");
                Debug.LogError($"Response body: {request.downloadHandler.text}");
                Debug.LogError($"=====================================");
            }
        }

        Debug.Log($"POST completat, carregantS escena...");

        LoadSceneByRated(menuSceneName, ratingSceneName);
        isPostingClassification = false;
    }

    private void LoadSceneByRated(string menuSceneName, string ratingSceneName)
    {
        Debug.Log($"===== LOAD SCENE BY RATED =====");
        Debug.Log($"verifyResponse: {(verifyResponse != null ? "EXISTS" : "NULL")}");
        
        if (verifyResponse != null)
        {
            Debug.Log($"verifyResponse.rated: {verifyResponse.rated}");
        }
        
        bool userRated = IsUserRated();
        string nextScene = userRated ? menuSceneName : ratingSceneName;
        
        Debug.Log($"IsUserRated() = {userRated}");
        Debug.Log($"MenuScene: {menuSceneName}, RatingScene: {ratingSceneName}");
        Debug.Log($"Carregant escena: '{nextScene}'");
        Debug.Log($"==============================");
        
        SceneManager.LoadScene(nextScene);
    }

    // Getters per accedir a les dades d'usuari i verify des d'altres scripts
    public string GetCurrentUserName() => currentUserName;
    public string GetCurrentUserEmail() => currentUserEmail;
    public VerifyResponseDTO GetVerifyResponse() => verifyResponse;
    public bool IsUserRated() => verifyResponse != null && verifyResponse.rated;

    public bool TryGetCurrentCredentials(out string userName, out string userEmail)
    {
        if (string.IsNullOrWhiteSpace(currentUserName) || string.IsNullOrWhiteSpace(currentUserEmail))
            LoadSessionFromPrefs();

        userName = currentUserName;
        userEmail = currentUserEmail;

        return !string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(userEmail);
    }

    private void SaveSessionToPrefs()
    {
        if (!string.IsNullOrWhiteSpace(currentUserName))
            PlayerPrefs.SetString(UserNameKey, currentUserName);

        if (!string.IsNullOrWhiteSpace(currentUserEmail))
            PlayerPrefs.SetString(UserEmailKey, currentUserEmail);

        if (verifyResponse != null)
            PlayerPrefs.SetInt(UserRatedKey, verifyResponse.rated ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void LoadSessionFromPrefs()
    {
        if (string.IsNullOrWhiteSpace(currentUserName))
            currentUserName = PlayerPrefs.GetString(UserNameKey, "");

        if (string.IsNullOrWhiteSpace(currentUserEmail))
            currentUserEmail = PlayerPrefs.GetString(UserEmailKey, "");

        if (verifyResponse == null && PlayerPrefs.HasKey(UserRatedKey))
        {
            verifyResponse = new VerifyResponseDTO
            {
                rated = PlayerPrefs.GetInt(UserRatedKey, 0) == 1,
                criterion = new System.Collections.Generic.List<CriterionDTO>()
            };
        }
    }

    private bool EnsureNetworkingData()
    {
        if (networkingData != null)
            return true;

        networkingData = Resources.Load<NetworkingData>("NetworkingData");

        if (networkingData == null)
        {
            Debug.LogError("No se pudo cargar 'Resources/NetworkingData'. Verifica que el asset esté en Assets/Resources/NetworkingData.asset");
            return false;
        }

        return true;
    }
}
