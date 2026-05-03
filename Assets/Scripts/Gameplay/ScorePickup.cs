using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objecte recollectable que suma punts al jugador quan hi entra en contacte i es destrueix.
/// Requereix un Collider amb "Is Trigger" activat.
/// </summary>
public class ScorePickup : MonoBehaviour
{
    [Header("Score")]
    [Tooltip("Punts que s'afegiran al jugador en recollir l'objecte.")]
    public float pointsToAdd = 100f;

    [Tooltip("Nom identificador que apareixerà al log de puntuació.")]
    public string pickupName = "Pickup";

    [Tooltip("Id unic del pickup per al log. Si queda buit s'utilitza el nom de l'objecte.")]
    public string pickupId = "";

    [Tooltip("Id de zona del pickup (opcional). Si queda buit es resol amb la zona activa en recollir.")]
    public string zoneId = "";

    [Header("Animation")]
    [Tooltip("Velocitat de rotació")]
    public Vector3 rotationSpeed = new Vector3(0, 120, 0);

    [Tooltip("Altura de la flotació")]
    public float floatAmplitude = 0.25f;

    [Tooltip("Velocitat de la flotació")]
    public float floatSpeed = 1.5f;

    [Header("Narrativa")]
    public List<string> narratorLines;
    private Vector3 startPos;
    private float offset;

    

    void Start()
    {
        if (string.IsNullOrWhiteSpace(pickupId))
        {
            pickupId = gameObject.name;
        }

        startPos = transform.position;

        // Desfase aleatorio para que no todas floten igual
        offset = Random.Range(0f, Mathf.PI * 2f);

        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.RegisterPickup(pickupId, pickupName, zoneId);
        }
    }

    void Update()
    {
        // ROTACIÓN
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // FLOTACIÓN
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed + offset) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Score.Instance != null)
            Score.Instance.AddPoints(pointsToAdd, pickupName);

        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.MarkPickupCollected(pickupId, pickupName, zoneId);
        }
        if (narratorLines.Count > 0 && NarratorUI.Instance != null)
{
    NarratorUI.Instance.TriggerDialogue(narratorLines);
}

        Destroy(gameObject);
    }

    
}