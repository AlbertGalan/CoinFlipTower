using UnityEngine;

[CreateAssetMenu(fileName = "NetworkingData", menuName = "Scriptable Objects/NetworkingData")]
public class NetworkingData : ScriptableObject
{
    [SerializeField] private string apiUrl = "https://phpstack-1076337-5399863.cloudwaysapps.com/api";
    [SerializeField] private string api_token;

    public string ApiUrl => apiUrl;
    public string ApiToken => api_token;
}
