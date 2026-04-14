using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameSessionLogger : MonoBehaviour
{
    [Serializable]
    public class ZoneLogData
    {
        public string zoneId;
        public float timeSpent;
        public int entries;
        public int playerFlips;
        public int objectFlips;
    }

    [Serializable]
    public class ZoneVisitLogData
    {
        public string zoneId;
        public int visitIndexForZone;
        public string enteredAt;
        public string exitedAt;
        public float duration;
        public int playerFlips;
        public int objectFlips;
    }

    [Serializable]
    public class GrabLogData
    {
        public string objectName;
        public float duration;
        public string zoneId;
        public string timestamp;
    }

    [Serializable]
    public class EasterEggLogData
    {
        public string pickupId;
        public string pickupName;
        public bool collected;
        public string zoneId;
        public string collectedAt;
    }

    [Serializable]
    public class SessionLogData
    {
        public string sessionId;
        public string startTime;
        public string endTime;
        public float totalPlaytime;
        public int totalScore;

        public List<ZoneVisitLogData> zoneVisits = new List<ZoneVisitLogData>();
        public List<ZoneLogData> zones = new List<ZoneLogData>();
        public List<GrabLogData> grabbedObjects = new List<GrabLogData>();
        public List<EasterEggLogData> easterEggs = new List<EasterEggLogData>();
    }

    public static GameSessionLogger Instance { get; private set; }

    private readonly Dictionary<string, ZoneLogData> zoneMap = new Dictionary<string, ZoneLogData>();
    private readonly Dictionary<string, EasterEggLogData> pickupMap = new Dictionary<string, EasterEggLogData>();
    private readonly Dictionary<string, int> zoneVisitCount = new Dictionary<string, int>();

    private SessionLogData data;
    private bool hasSaved;

    private string activeZoneId;
    private float activeZoneEnterTime;
    private ZoneVisitLogData currentVisit;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("GameSessionLogger");
        go.AddComponent<GameSessionLogger>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeNewSession();
    }

    /// <summary>
    /// Reinicia la sessió de registre, esborrant totes les dades anteriors i començant una nova amb un nou ID de sessió i hora d'inici. Això és útil per a reiniciar el joc o començar una nova partida sense tancar l'aplicació.
    /// </summary>
    public void InitializeNewSession()
    {
        zoneMap.Clear();
        pickupMap.Clear();
        zoneVisitCount.Clear();

        data = new SessionLogData
        {
            sessionId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"),
            startTime = DateTime.UtcNow.ToString("o")
        };

        activeZoneId = null;
        activeZoneEnterTime = 0f;
        currentVisit = null;
        hasSaved = false;

        Debug.Log($"[GameSessionLogger] Nueva sesión inicializada: {data.sessionId}");
    }

    public string GetCurrentZoneId()
    {
        return string.IsNullOrWhiteSpace(activeZoneId) ? "NoZone" : activeZoneId;
    }

    public void EnterZone(string zoneId)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
            zoneId = "NoZone";

        if (activeZoneId == zoneId)
            return;

        CloseCurrentVisit();

        activeZoneId = zoneId;
        activeZoneEnterTime = Time.time;

        int nextVisitIndex = 1;
        if (zoneVisitCount.TryGetValue(zoneId, out int previousCount))
            nextVisitIndex = previousCount + 1;

        zoneVisitCount[zoneId] = nextVisitIndex;

        currentVisit = new ZoneVisitLogData
        {
            zoneId = zoneId,
            visitIndexForZone = nextVisitIndex,
            enteredAt = DateTime.UtcNow.ToString("o"),
            exitedAt = string.Empty,
            duration = 0f,
            playerFlips = 0,
            objectFlips = 0
        };

        data.zoneVisits.Add(currentVisit);
        EnsureZone(zoneId).entries++;
    }

    public void ExitZone(string zoneId)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
            return;

        if (activeZoneId != zoneId)
            return;

        CloseCurrentVisit();
        activeZoneId = null;
    }

    public void LogGravityFlip(bool isPlayerFlip)
    {
        ZoneLogData zone = EnsureZone(GetCurrentZoneId());

        if (isPlayerFlip)
            zone.playerFlips++;
        else
            zone.objectFlips++;

        if (currentVisit != null)
        {
            if (isPlayerFlip)
                currentVisit.playerFlips++;
            else
                currentVisit.objectFlips++;
        }
    }

    public void LogGrabEvent(string objectName, float duration)
    {
        if (duration < 0f) duration = 0f;

        data.grabbedObjects.Add(new GrabLogData
        {
            objectName = string.IsNullOrWhiteSpace(objectName) ? "UnnamedObject" : objectName,
            duration = duration,
            zoneId = GetCurrentZoneId(),
            timestamp = DateTime.UtcNow.ToString("o")
        });
    }

    public void RegisterPickup(string pickupId, string pickupName, string zoneId = "")
    {
        if (string.IsNullOrWhiteSpace(pickupId))
            return;

        if (pickupMap.ContainsKey(pickupId))
            return;

        EasterEggLogData entry = new EasterEggLogData
        {
            pickupId = pickupId,
            pickupName = string.IsNullOrWhiteSpace(pickupName) ? pickupId : pickupName,
            collected = false,
            zoneId = string.IsNullOrWhiteSpace(zoneId) ? "Unknown" : zoneId,
            collectedAt = string.Empty
        };

        pickupMap[pickupId] = entry;
        data.easterEggs.Add(entry);
    }

    public void MarkPickupCollected(string pickupId, string pickupName, string zoneId = "")
    {
        if (string.IsNullOrWhiteSpace(pickupId))
            return;

        if (!pickupMap.TryGetValue(pickupId, out EasterEggLogData entry))
        {
            RegisterPickup(pickupId, pickupName, zoneId);
            if (!pickupMap.TryGetValue(pickupId, out entry))
                return;
        }

        entry.collected = true;
        entry.collectedAt = DateTime.UtcNow.ToString("o");

        if (!string.IsNullOrWhiteSpace(pickupName))
            entry.pickupName = pickupName;

        if (!string.IsNullOrWhiteSpace(zoneId))
            entry.zoneId = zoneId;
        else if (string.IsNullOrWhiteSpace(entry.zoneId) || entry.zoneId == "Unknown")
            entry.zoneId = GetCurrentZoneId();
    }

    public void SaveFinalLog(int finalScore, float totalPlaytime)
    {
        if (hasSaved)
            return;

        CloseCurrentVisit();

        data.totalScore = finalScore;
        data.totalPlaytime = totalPlaytime < 0f ? Time.timeSinceLevelLoad : totalPlaytime;
        data.endTime = DateTime.UtcNow.ToString("o");

        string json = JsonUtility.ToJson(data, true);

        string folder = Application.streamingAssetsPath;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string fileName = $"gameplay_log_{data.sessionId}.json";
        string path = Path.Combine(folder, fileName);
        File.WriteAllText(path, json);

        hasSaved = true;
        Debug.Log($"GameSessionLogger guardat a: {path}");
    }

    private void OnApplicationQuit()
    {
        if (hasSaved)
            return;

        int score = Score.Instance != null ? Score.Instance.GetScoreInt() : Mathf.RoundToInt(Score.GetLastScore());
        float playtime = Score.Instance != null ? Score.Instance.GetTimer() : Time.timeSinceLevelLoad;
        SaveFinalLog(score, playtime);
    }

    private ZoneLogData EnsureZone(string zoneId)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
            zoneId = "NoZone";

        if (zoneMap.TryGetValue(zoneId, out ZoneLogData zone))
            return zone;

        zone = new ZoneLogData
        {
            zoneId = zoneId,
            timeSpent = 0f,
            entries = 0,
            playerFlips = 0,
            objectFlips = 0
        };

        zoneMap[zoneId] = zone;
        data.zones.Add(zone);
        return zone;
    }

    private void CloseCurrentVisit()
    {
        if (string.IsNullOrWhiteSpace(activeZoneId) || currentVisit == null)
            return;

        float elapsed = Time.time - activeZoneEnterTime;
        if (elapsed <= 0f)
            elapsed = 0f;

        ZoneLogData zone = EnsureZone(activeZoneId);
        zone.timeSpent += elapsed;

        currentVisit.duration += elapsed;
        currentVisit.exitedAt = DateTime.UtcNow.ToString("o");
        currentVisit = null;
    }
}
