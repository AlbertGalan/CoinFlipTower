using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ClassificationManager : MonoBehaviour
{
    [SerializeField] private NetworkingData networkingData;
    [SerializeField] private Transform contentParent; // Donde aparecen las filas
    [SerializeField] private GameObject classificationRowPrefab; // Prefab de la fila
    [SerializeField] private Button closeButton; // Botón para cerrar el panel

    void Start()
    {
        // Inicializar NetworkingData
        if (networkingData == null)
            networkingData = Resources.Load<NetworkingData>("NetworkingData");
        
        // Conectar botón de cerrar
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        // Cargar clasificación cuando el panel se abre
        //StartCoroutine(GetClassificationCoroutine());
            StartCoroutine(LoadAfterFrame());

    }
private IEnumerator LoadAfterFrame()
{
    yield return null; // Espera un frame
    StartCoroutine(GetClassificationCoroutine());
}
    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator GetClassificationCoroutine()
    {
        if (networkingData == null)
        {
            Debug.LogError("NetworkingData no configurado");
            yield break;
        }

        string url = $"{networkingData.ApiUrl}/classification/{networkingData.ApiToken}";
        Debug.Log("URL de clasificación: " + url);
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta JSON: " + jsonResponse);
                
                ClassificationResponseDTO response = JsonConvert.DeserializeObject<ClassificationResponseDTO>(jsonResponse);
                
                if (response != null && response.data != null)
                {
                    Debug.Log($"Total de clasificaciones recibidas: {response.data.Count}");
                    foreach (var score in response.data)
                    {
                        Debug.Log($"Nombre: {score.name}, Puntuación: {score.puntuacion}");
                    }
                }
                else
                {
                    Debug.LogWarning("La respuesta o data es null");
                }
                
                // Actualizar UI
                DisplayClassification(response.data);
            }
            else
            {
                Debug.LogError($"Error al obtendre la classificació: {request.error} - Código: {request.responseCode}");
            }
        }
    }

    private void DisplayClassification(List<ScoreDTO> scores)
    {
        Debug.Log("DisplayClassification llamado");
        
        if (scores == null)
        {
            Debug.LogWarning("scores es null");
            return;
        }
        
        if (contentParent == null)
        {
            Debug.LogError("contentParent no está asignado en el Inspector");
            return;
        }
        
        if (classificationRowPrefab == null)
        {
            Debug.LogError("classificationRowPrefab no está asignado en el Inspector");
            return;
        }

        Debug.Log($"Mostrando {scores.Count} clasificaciones");

        for (int i = 0; i < scores.Count; i++)
        {
            ScoreDTO score = scores[i];
            Debug.Log($"[UI] Posición {i + 1}: {score.name} - {score.puntuacion}");
        }

        // Limpiar filas previas
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevas filas
        for (int i = 0; i < scores.Count; i++)
        {
            ScoreDTO score = scores[i];
            GameObject rowInstance = Instantiate(classificationRowPrefab, contentParent);
            Debug.Log($"Fila creada: {score.name} - {score.puntuacion}");
            
            ClassificationRowUI rowUI = rowInstance.GetComponent<ClassificationRowUI>();
            if (rowUI != null)
            {
                rowUI.SetData(i + 1, score.name, score.puntuacion);
            }
            else
            {
                Debug.LogWarning("classificationRowPrefab no tiene el componente ClassificationRowUI");
            }
        }
    }
    
}

