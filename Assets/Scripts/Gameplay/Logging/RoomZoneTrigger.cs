using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomZoneTrigger : MonoBehaviour
{
    [Tooltip("Identificador unic de la sala o zona")]
    public string zoneId = "Room_01";

    [Tooltip("Tag que activa l'entrada/sortida de zona")]
    public string actorTag = "Player";

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(actorTag)) return;

        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.EnterZone(zoneId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(actorTag)) return;

        if (GameSessionLogger.Instance != null)
        {
            GameSessionLogger.Instance.ExitZone(zoneId);
        }
    }
}
