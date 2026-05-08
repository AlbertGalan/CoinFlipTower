using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objecte recollectable que suma punts al jugador quan hi entra en contacte i es destrueix.
/// </summary>
public class ScorePickup : MonoBehaviour
{
    [Header("Score")]
    public float pointsToAdd = 100f;
    public string pickupName = "Pickup";
    public string pickupId = "";
    public string zoneId = "";

    [Header("Audio")]
    [Tooltip("Clip de so opcional que es reproduirà en recollir l'objecte.")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float volume = 0.7f;

    [Header("Animation")]
    public Vector3 rotationSpeed = new Vector3(0, 120, 0);
    public float floatAmplitude = 0.25f;
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

        // --- LÓGICA DE AUDIO ---
        if (pickupSound != null)
        {
            // Reproduce el sonido en la posición actual antes de destruir el objeto
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
        }

        // --- PUNTOS Y LOGS ---
        if (Score.Instance != null)
            Score.Instance.AddPoints(pointsToAdd, pickupName);

        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.MarkPickupCollected(pickupId, pickupName, zoneId);
        }

        // --- NARRATIVA ---
        if (narratorLines.Count > 0 && NarratorUI.Instance != null)
        {
            NarratorUI.Instance.TriggerDialogue(narratorLines);
        }

        Destroy(gameObject);
    }
}