using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;
    private Vector3 lastCheckpointPosition;

    void Awake()
    {
        Instance = this;
        // Posición inicial por defecto al empezar el nivel
        lastCheckpointPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
    }

    public void SetCheckpoint(Vector3 pos) => lastCheckpointPosition = pos;

    public Vector3 GetLastCheckpoint() => lastCheckpointPosition;
}