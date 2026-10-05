using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TilePuzzleManager : MonoBehaviour
{
    public UnityEvent onPuzzleComplete;
    
    private List<PuzzleTile> allTiles = new List<PuzzleTile>();
    private int activatedCount = 0;
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
        Debug.Log("¡Puzzle Resuelto!");
    }

    public void TriggerFailure(GameObject player)
    {
        if (IsComplete) return;
        StartCoroutine(FailureSequence(player));
    }

    IEnumerator FailureSequence(GameObject player)
    {
        var moveScript = player.GetComponent<MoveCharacter>();
        if (moveScript != null) moveScript.SetBlockMovement(true);

        yield return new WaitForSeconds(0.2f);

        // Limpiamos las baldosas
        ResetPuzzle();

        // Respawn en el centro de la sala (según el transform del Checkpoint activo)
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer(player);
        }

        yield return new WaitForSeconds(0.4f);

        if (moveScript != null) moveScript.SetBlockMovement(false);
    }

    public void ResetPuzzle()
    {
        if (IsComplete) return; 

        activatedCount = 0;
        foreach (var tile in allTiles)
        {
            tile.SetState(PuzzleTile.TileState.Off);
        }
    }
}