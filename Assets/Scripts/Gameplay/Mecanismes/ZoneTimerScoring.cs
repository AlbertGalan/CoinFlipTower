using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZoneTimerScoring : MonoBehaviour
{
    [Header("Detecció")]
    [Tooltip("Tags que activen el sistema de zona")] 
    public List<string> targetTags = new List<string> { "Player" };

    [Header("Vinculació de zona")]
    [Tooltip("Identificador per enllaçar el trigger d'entrada i el de sortida")] 
    public string zoneId = "DefaultZone";
    [Tooltip("True per al trigger d'entrada; False per al trigger de sortida")] 
    public bool isEntryTrigger = true;

    [Header("Temps límit")]
    [Tooltip("Temps màxim per aconseguir puntuació d'or (segons)")] 
    public float goldTime = 10f;
    [Tooltip("Temps màxim per aconseguir puntuació de plata (segons)")] 
    public float silverTime = 15f;

    [Tooltip("Temps màxim per aconseguir puntuació de bronze (segons)")] 
    public float bronzeTime = 20f;

    [Header("Puntuacions")]
    [Tooltip("Punts atorgats si es completa abans de goldTime")] 
    public int goldPoints = 100;

    [Tooltip("Punts atorgats si es completa abans de silverTime")] 
    public int silverPoints = 50;
    
    [Tooltip("Punts atorgats si es completa abans de bronzeTime")] 
    public int bronzePoints = 25;

    [Header("Score destí")]
    [Tooltip("Component Score al que se sumaran punts (es detecta automàticament si està buit)")] 
    public Score score;

    [Header("Esdeveniments")]
    public UnityEvent OnZoneStarted;
    public UnityEvent OnZoneCompleted;

    [Header("Destrucció")]
    [Tooltip("Destruir objectes amb el mateix zoneId després de completar (només si isEntryTrigger = false)")]
    public bool destroyOnComplete = true;
    [Tooltip("Temps d'espera abans de destruir (segons)")]
    public float destroyDelay = 2f;

    // Almacenamos tiempos de inicio por (zoneId → actor → time)
    private static readonly Dictionary<string, Dictionary<GameObject, float>> startTimes =
        new Dictionary<string, Dictionary<GameObject, float>>();

    // Evita doble recompensa por la misma pareja (zona, actor)
    private static readonly HashSet<(string, GameObject)> awardedPairs =
        new HashSet<(string, GameObject)>();
    
    // Estado actual de la zona para el HUD
    public static ZoneTimerScoring activeZone = null;

    private bool IsTargetTag(string tag) => targetTags.Contains(tag);

    private void Awake()
    {
        if (score == null)
        {
            score = FindAnyObjectByType<Score>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTargetTag(other.tag)) return;

        if (isEntryTrigger)
        {
            StartZoneFor(other.gameObject);
        }
        else
        {
            CompleteZoneFor(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTargetTag(other.tag)) return;

        if (!isEntryTrigger)
        {
            // Si prefieres otorgar al salir del trigger de salida en vez de entrar
            CompleteZoneFor(other.gameObject);
        }
    }

    private void StartZoneFor(GameObject actor)
    {
        if (!startTimes.TryGetValue(zoneId, out var actors))
        {
            actors = new Dictionary<GameObject, float>();
            startTimes[zoneId] = actors;
        }

        actors[actor] = Time.time;
        
        // Establecer zona activa para el HUD
        if (actor.CompareTag("Player"))
        {
            activeZone = this;
        }
        
        Debug.Log($"Zona '{zoneId}' iniciada per {actor.name} a {actors[actor]:F2}s");
        OnZoneStarted?.Invoke();
    }

    private void CompleteZoneFor(GameObject actor)
    {
        if (!startTimes.TryGetValue(zoneId, out var actors) || !actors.TryGetValue(actor, out var start))
        {
            Debug.LogWarning($"Zona '{zoneId}' sortida assolida per {actor.name} sense haver passat per entrada.");
            return;
        }

        var key = (zoneId, actor);
        if (awardedPairs.Contains(key))
        {
            Debug.Log($"Zona '{zoneId}' ja premiada per {actor.name}.");
            return;
        }

        float elapsed = Time.time - start;
        int points = GetPointsForElapsed(elapsed, out string rank);

        if (score != null)
        {
            score.AddPoints(points, rank);
            Debug.Log($"Zona '{zoneId}' completada per {actor.name} en {elapsed:F2}s → +{points} punts ({rank}). Total: {score.score:F0}");
        }
        else
        {
            Debug.LogWarning($"No s'ha trobat component Score per atorgar {points} punts.");
        }
        
        // Notificar al HUD per mostrar els punts
        if (actor.CompareTag("Player"))
        {
            HUD_Manager hudManager = FindAnyObjectByType<HUD_Manager>();
            if (hudManager != null)
            {
                hudManager.ShowChallengeComplete(points, rank);
            }
        }
        
        // Limpiar zona activa
        if (actor.CompareTag("Player") && activeZone == this)
        {
            activeZone = null;
        }

        awardedPairs.Add(key);
        actors.Remove(actor);
        OnZoneCompleted?.Invoke();
        
        // Destruir objectes amb el mateix zoneId si és el trigger de sortida
        if (!isEntryTrigger && destroyOnComplete)
        {
            DestroyZoneObjects();
        }
    }

    private void DestroyZoneObjects()
    {
        // Buscar tots els ZoneTimerScoring amb el mateix zoneId
        ZoneTimerScoring[] allZones = FindObjectsByType<ZoneTimerScoring>(FindObjectsSortMode.None);
        
        foreach (ZoneTimerScoring zone in allZones)
        {
            if (zone.zoneId == this.zoneId)
            {
                Debug.Log($"Destruint objecte '{zone.gameObject.name}' de zona '{zoneId}' en {destroyDelay}s");
                Destroy(zone.gameObject, destroyDelay);
            }
        }
    }

    private int GetPointsForElapsed(float elapsed, out string rank)
    {
        if (elapsed <= goldTime)
        {
            rank = "OR";
            return goldPoints;
        }
        else if (elapsed <= silverTime)
        {
            rank = "PLATA";
            return silverPoints;
        }
        else if (elapsed <= bronzeTime)
        {
            rank = "BRONZE";
            return bronzePoints;
        }
        else
        {
            rank = "BRONZE";
            return bronzePoints; // Puntuació mínima si excedeix tots els temps
        }
    }
    
    // Obtener tiempo transcurrido para el HUD
    public float GetElapsedTime(GameObject actor)
    {
        if (startTimes.TryGetValue(zoneId, out var actors) && actors.TryGetValue(actor, out var start))
        {
            return Time.time - start;
        }
        return 0f;
    }
}
