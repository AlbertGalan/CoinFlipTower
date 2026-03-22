using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LogicPuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public class ExpectedPlacement
    {
        public LogicPuzzleSlot slot;
        public LogicPuzzleSymbol expectedSymbol = LogicPuzzleSymbol.None;
    }

    [Header("Solución")]
    [Tooltip("Asigna cada slot con el símbolo que debe contener")]
    public List<ExpectedPlacement> expectedPlacements = new List<ExpectedPlacement>();

    [Header("Comportamiento")]
    [Tooltip("Si está activo, una vez resuelto no vuelve a estado no resuelto")]
    public bool lockSolvedState = true;

    [Header("Feedback de audio (opcional)")]
    public AudioSource correctPlacementAudio;
    public AudioSource incorrectPlacementAudio;
    public AudioSource solvedAudio;

    [Header("Feedback de progreso (opcional)")]
    [Tooltip("Texto tipo 2/6")]
    public TMP_Text progressText;

    [Header("Eventos")]
    public UnityEvent OnPuzzleSolved;
    public UnityEvent OnPuzzleUnsolved;

    private bool isSolved;

    private void Start()
    {
        ReevaluateAllSlots(playPlacementAudio: false);
    }

    public void NotifySlotChanged(LogicPuzzleSlot changedSlot)
    {
        Debug.Log($"[Manager] NotifySlotChanged: {(changedSlot != null ? changedSlot.slotId : "null")}");
        
        if (changedSlot == null)
        {
            ReevaluateAllSlots(playPlacementAudio: false);
            return;
        }

        EvaluateSingleSlot(changedSlot, playAudio: true);
        ReevaluateAllSlots(playPlacementAudio: false);
    }

    private void ReevaluateAllSlots(bool playPlacementAudio)
    {
        int total = 0;
        int correct = 0;
        bool allFilled = true;

        Debug.Log("[Manager] Reevaluando todos los slots...");

        for (int i = 0; i < expectedPlacements.Count; i++)
        {
            ExpectedPlacement placement = expectedPlacements[i];
            if (placement == null || placement.slot == null)
            {
                Debug.LogWarning($"[Manager] Placement {i} está vacío o sin slot asignado");
                continue;
            }

            total++;
            LogicPuzzlePiece piece = placement.slot.CurrentPiece;
            LogicPuzzleSymbol currentSymbol = piece != null ? piece.Symbol : LogicPuzzleSymbol.None;
            Debug.Log($"[Manager] Slot {i} ({placement.slot.slotId}): Espera={placement.expectedSymbol}, Actual={currentSymbol}");
            
            bool isCorrect = EvaluateSingleSlot(placement.slot, playPlacementAudio);
            if (isCorrect)
            {
                correct++;
            }
            else if (placement.slot.CurrentPiece == null)
            {
                allFilled = false;
            }
        }

        Debug.Log($"[Manager] Progreso: {correct}/{total} correctos, AllFilled={allFilled}");

        if (progressText != null)
        {
            progressText.text = $"{correct}/{total}";
        }

        bool shouldBeSolved = total > 0 && correct == total && allFilled;
        if (lockSolvedState && isSolved)
        {
            return;
        }

        if (shouldBeSolved && !isSolved)
        {
            isSolved = true;
            if (solvedAudio != null) solvedAudio.Play();
            OnPuzzleSolved.Invoke();
            Debug.Log("Puzzle lógico resuelto");
        }
        else if (!shouldBeSolved && isSolved)
        {
            isSolved = false;
            OnPuzzleUnsolved.Invoke();
            Debug.Log("Puzzle lógico desactivado");
        }
    }

    private bool EvaluateSingleSlot(LogicPuzzleSlot slot, bool playAudio)
    {
        ExpectedPlacement expected = FindExpectedPlacement(slot);
        if (expected == null)
        {
            Debug.LogWarning($"[Manager] No se encontró placement esperado para slot {slot.slotId}");
            return false;
        }

        LogicPuzzlePiece piece = slot.CurrentPiece;
        if (piece == null)
        {
            Debug.Log($"[Manager] Slot {slot.slotId} vacío");
            slot.SetFeedbackState(LogicPuzzleSlot.FeedbackState.Neutral);
            return false;
        }

        bool isCorrect = piece.Symbol == expected.expectedSymbol;
        Debug.Log($"[Manager] Validando {slot.slotId}: {piece.Symbol} vs esperado {expected.expectedSymbol} = {isCorrect}");
        slot.SetFeedbackState(isCorrect ? LogicPuzzleSlot.FeedbackState.Correct : LogicPuzzleSlot.FeedbackState.Incorrect);

        if (playAudio)
        {
            if (isCorrect && correctPlacementAudio != null)
            {
                correctPlacementAudio.Play();
            }
            else if (!isCorrect && incorrectPlacementAudio != null)
            {
                incorrectPlacementAudio.Play();
            }
        }

        return isCorrect;
    }

    private ExpectedPlacement FindExpectedPlacement(LogicPuzzleSlot slot)
    {
        for (int i = 0; i < expectedPlacements.Count; i++)
        {
            ExpectedPlacement placement = expectedPlacements[i];
            if (placement != null && placement.slot == slot)
            {
                return placement;
            }
        }

        return null;
    }

    public bool IsSolved()
    {
        return isSolved;
    }
}