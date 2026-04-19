using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoldierPuzzleManager : MonoBehaviour
{
    [Header("Slots")]
    public SoldierPuzzleSlot blueSlot;
    public SoldierPuzzleSlot redSlot;

    [Header("Acciones al Resolver")]
    public UnityEvent OnPuzzleSolved;
    private bool isSolved = false;

    [Header("Configuración de Limpieza")]
    [Tooltip("Si la lista está vacía, se llenará automáticamente con todos los objetos con el Tag 'Soldier' al iniciar.")]
    public List<GameObject> soldiers;

    private void Awake()
    {
        // Optimizamos: Buscamos los soldados UNA SOLA VEZ al cargar la escena
        // Solo si no los has arrastrado manualmente al Inspector
        if (soldiers == null || soldiers.Count == 0)
        {
            soldiers = new List<GameObject>(GameObject.FindGameObjectsWithTag("Soldier"));
            Debug.Log($"<color=cyan>SoldierPuzzleManager:</color> Se han encontrado {soldiers.Count} soldados para limpiar.");
        }
    }

    public void NotifySlotChanged()
    {
        if (isSolved) return;

        // Comprobamos los slots contra su color configurado en Inspector.
        bool blueCorrect = CheckSlot(blueSlot);
        bool redCorrect = CheckSlot(redSlot);

        if (blueCorrect && redCorrect)
        {
            SolvePuzzle();
        }
    }

    private bool CheckSlot(SoldierPuzzleSlot slot)
    {
        if (slot == null) return false;

        if (slot.CurrentBlock == null)
        {
            slot.ApplyFeedbackColor(SoldierPuzzleSlot.FeedbackState.Neutral);
            return false;
        }

        bool match = slot.CurrentBlock.blockColor == slot.requiredColor;
        slot.ApplyFeedbackColor(match ? SoldierPuzzleSlot.FeedbackState.Correct : SoldierPuzzleSlot.FeedbackState.Incorrect);
        return match;
    }

    private void SolvePuzzle()
    {
        isSolved = true;
        Debug.Log("<color=green><b>[PUZZLE RESUELTO]</b></color> Destruyendo soldados y ejecutando eventos.");

        DestroyAllSoldiers();

        // 3. Ejecutar eventos adicionales (abrir puertas, sonidos, etc.)
        OnPuzzleSolved?.Invoke();
    }

    private void DestroyAllSoldiers()
    {
        if (soldiers == null) return;

        foreach (GameObject soldier in soldiers)
        {
            if (soldier != null)
            {
                Destroy(soldier);
            }
        }
        
        soldiers.Clear();
    }

}