using UnityEngine;

public class FlappyCheckpoint : MonoBehaviour
{
    public int checkpointIndex; // 0, 1 o 2
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            FindFirstObjectByType<FlappyPuzzleManager>().SetCheckpoint(checkpointIndex);
        }
    }
}