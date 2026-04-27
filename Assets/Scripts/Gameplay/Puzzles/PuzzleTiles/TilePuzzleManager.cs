using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TilePuzzleManager : MonoBehaviour
{
    public UnityEvent onPuzzleComplete;
    
    private List<PuzzleTile> allTiles = new List<PuzzleTile>();
    private int activatedCount = 0;
    
    // Propiedad pública para que las baldosas consulten el estado
    public bool IsComplete { get; private set; }

    void Start()
    {
        allTiles.AddRange(GetComponentsInChildren<PuzzleTile>());
        IsComplete = false;
    }

    public void OnTileActivated()
    {
        if (IsComplete) return;

        activatedCount++;

        if (activatedCount >= allTiles.Count)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        IsComplete = true;
        onPuzzleComplete.Invoke();
        Debug.Log("¡Puzzle Resuelto! Las baldosas ya no matarán.");
    }

    public void TriggerFailure(GameObject player)
    {
        // Si por algún milagro se pisa después de ganar pero antes de procesar,
        // esta línea salva al jugador
        if (IsComplete) return;

        StartCoroutine(FailureSequence(player));
    }

    IEnumerator FailureSequence(GameObject player)
    {
        var moveScript = player.GetComponent<MoveCharacter>();
        if (moveScript != null) moveScript.SetBlockMovement(true);

        yield return new WaitForSeconds(0.2f);

        ResetPuzzle();

        Rigidbody rb = player.GetComponent<Rigidbody>();
        Vector3 spawnPos = CheckpointManager.Instance.GetLastCheckpoint();
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.position = spawnPos;
        }
        player.transform.position = spawnPos;

        yield return new WaitForSeconds(0.4f);

        if (moveScript != null) moveScript.SetBlockMovement(false);
    }

    public void ResetPuzzle()
    {
        if (IsComplete) return; // No resetear si ya ganó

        activatedCount = 0;
        foreach (var tile in allTiles)
        {
            tile.SetState(PuzzleTile.TileState.Off);
        }
    }
}