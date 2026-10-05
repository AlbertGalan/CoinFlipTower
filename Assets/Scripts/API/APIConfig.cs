using UnityEngine;

[CreateAssetMenu(fileName = "APIConfig", menuName = "Config/APIConfig")]
public class APIConfig : ScriptableObject
{
    private static APIConfig instance;
    private static bool hasTriedToLoad = false;

    public static APIConfig Instance
    {
        get
        {
            if (instance != null)
                return instance;

            if (!hasTriedToLoad)
            {
                hasTriedToLoad = true;
                instance = Resources.Load<APIConfig>("APIConfig");
            }

            if (instance == null)
            {
                Debug.LogWarning("APIConfig no trobat en Resources/APIConfig.asset. Usant valors per defecte.");
                instance = CreateInstance<APIConfig>();
                instance.enableAPI = true;
                instance.showDevLogs = true;
                instance.simulatedNetworkDelay = 0.5f;
            }

            return instance;
        }
    }

    [Tooltip("Habilita o deshabilita totes les crides a la API")]
    public bool enableAPI = true;

    [Tooltip("Si enableAPI=false, mostra els logs de desenvolupament")]
    public bool showDevLogs = true;

    [Tooltip("Temps de delay simulat per a crides API (en segundos)")]
    public float simulatedNetworkDelay = 0.5f;

    private void OnEnable()
    {
        instance = this;
    }

    public bool IsAPIEnabled()
    {
        return enableAPI;
    }

    public void Log(string message)
    {
        if (showDevLogs || enableAPI)
        {
            Debug.Log($"[APIConfig] {message}");
        }
    }

    public void LogWarning(string message)
    {
        if (showDevLogs || enableAPI)
        {
            Debug.LogWarning($"[APIConfig] {message}");
        }
    }
}
