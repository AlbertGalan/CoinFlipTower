using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoldierPuzzleManager : MonoBehaviour
{
    [Header("Slots")]
    public SoldierPuzzleSlot blueSlot;
    public SoldierPuzzleSlot redSlot;

    [Header("Acciones al Resolver")]
    public GameObject mechanismToOpen; // Puerta o escalera
    public bool destroyAllSoldiersOnSolve = true;

    public UnityEvent OnPuzzleSolved;
    private bool isSolved = false;

    public void NotifySlotChanged()
    {
        if (isSolved) return;

        bool blueCorrect = CheckSlot(blueSlot, DestructibleBlock.BlockColor.Blue);
        bool redCorrect = CheckSlot(redSlot, DestructibleBlock.BlockColor.Red);

        if (blueCorrect && redCorrect)
        {
            SolvePuzzle();
        }
    }

private bool CheckSlot(SoldierPuzzleSlot slot, DestructibleBlock.BlockColor expected)
{
    if (slot.CurrentBlock == null)
    {
        slot.ApplyFeedbackColor(SoldierPuzzleSlot.FeedbackState.Neutral);
        return false;
    }

    // Si el bloque es Rojo y esperábamos Azul, match será FALSE
    bool match = slot.CurrentBlock.blockColor == expected;
    
    // ESTA LÍNEA ES LA QUE ACTIVA EL COLOR
    slot.ApplyFeedbackColor(match ? SoldierPuzzleSlot.FeedbackState.Correct : SoldierPuzzleSlot.FeedbackState.Incorrect);
    
    return match;
}

    private void SolvePuzzle()
    {
        isSolved = true;
        Debug.Log("<color=green>Puzzle Resuelto!</color>");

        // 1. Abrir camino
        if (mechanismToOpen != null) mechanismToOpen.SetActive(false); // O llamar a una animación

        // 2. Destruir soldados
        if (destroyAllSoldiersOnSolve)
        {
            SoldierAI[] soldiers = Object.FindObjectsByType<SoldierAI>(FindObjectsSortMode.None);
            foreach (SoldierAI s in soldiers) Destroy(s.gameObject);
        }

        OnPuzzleSolved.Invoke();
    }
}