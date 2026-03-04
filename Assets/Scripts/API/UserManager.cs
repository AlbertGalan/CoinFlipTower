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

    // Datos del usuario actual
    private string currentUserName;
    private string currentUserEmail;

    // Respuesta del verify
    private VerifyResponseDTO verifyResponse;

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
    }

    private void Start()
    {
        // Cargar NetworkingData si no está asignado
        if (networkingData == null)
            networkingData = Resources.Load<NetworkingData>("NetworkingData");
    }

    /// <summary>
    /// Verifica el usuario con la API y, si es exitoso, inicia la partida
    /// </summary>
    public void VerifyAndStartGame(string userName, string userEmail)
    {
        currentUserName = userName;
        currentUserEmail = userEmail;
        StartCoroutine(VerifyUserCoroutine());
    }

    private IEnumerator VerifyUserCoroutine()
    {
        if (networkingData == null)
        {
            Debug.LogError("NetworkingData no configurado");
            yield break;
        }

        // Crear el objeto para serializar
        var verifyRequest = new
        {
            api_token = networkingData.ApiToken,
            name = currentUserName,
            email = currentUserEmail
        };

        string json = JsonConvert.SerializeObject(verifyRequest);
        string url = $"{networkingData.ApiUrl}/verify";

        Debug.Log($"POST a {url} con datos: {json}");

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
                Debug.Log("Respuesta del verify: " + jsonResponse);

                verifyResponse = JsonConvert.DeserializeObject<VerifyResponseDTO>(jsonResponse);

                if (verifyResponse != null)
                {
                    Debug.Log($"Usuario verificado. Rated: {verifyResponse.rated}");
                    Debug.Log($"Criterios disponibles: {verifyResponse.criterion.Count}");

                    foreach (var criterion in verifyResponse.criterion)
                    {
                        Debug.Log($"  - {criterion.name} ({criterion.min_score}-{criterion.max_score})");
                    }

                    // Iniciar la partida
                    StartGame();
                }
                else
                {
                    Debug.LogError("No se pudo deserializar la respuesta del verify");
                }
            }
            else
            {
                Debug.LogError($"Error en verify: {request.error} - Código: {request.responseCode}");
                Debug.LogError($"Respuesta: {request.downloadHandler.text}");
            }
        }
    }

    private void StartGame()
    {
        Debug.Log("Iniciando partida...");
        SceneManager.LoadScene("Tutorial");
    }

    // Getters para acceder a los datos desde otros managers
    public string GetCurrentUserName() => currentUserName;
    public string GetCurrentUserEmail() => currentUserEmail;
    public VerifyResponseDTO GetVerifyResponse() => verifyResponse;
    public bool IsUserRated() => verifyResponse != null && verifyResponse.rated;
}
