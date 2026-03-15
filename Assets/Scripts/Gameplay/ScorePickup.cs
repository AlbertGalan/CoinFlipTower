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

    [Header("Animation")]
    [Tooltip("Velocitat de rotació")]
    public Vector3 rotationSpeed = new Vector3(0, 120, 0);

    [Tooltip("Altura de la flotació")]
    public float floatAmplitude = 0.25f;

    [Tooltip("Velocitat de la flotació")]
    public float floatSpeed = 1.5f;

    private Vector3 startPos;
    private float offset;

    void Start()
    {
        startPos = transform.position;

        // Desfase aleatorio para que no todas floten igual
        offset = Random.Range(0f, Mathf.PI * 2f);
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

        Destroy(gameObject);
    }
}