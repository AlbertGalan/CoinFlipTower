using UnityEngine;

[CreateAssetMenu(fileName = "NetworkingData", menuName = "Scriptable Objects/NetworkingData")]
public class NetworkingData : ScriptableObject
{
    [SerializeField] private string apiUrl = "https://phpstack-1076337-5399863.cloudwaysapps.com/game";
    [SerializeField] private string token = "uZl9WgoE59y7c3JTN0dyj7KUxkKNP0MpS2NM8msPOZ4eUEtusumqYRHubOGS";

    public string ApiUrl => apiUrl;
    public string Token => token;
}
