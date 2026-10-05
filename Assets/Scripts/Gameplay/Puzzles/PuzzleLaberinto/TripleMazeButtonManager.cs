using UnityEngine;
using UnityEngine.Events;

public class TripleMazeButtonManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int totalButtons = 3;
    
    [Header("Eventos")]
    public UnityEvent onAllButtonsPressed;

    private int activatedButtonsCount = 0;

    public void NotifyButtonActivated()
    {
        activatedButtonsCount++;
        Debug.Log($"<color=cyan>Maze Puzzle:</color> Botones activados {activatedButtonsCount}/{totalButtons}");

        if (activatedButtonsCount >= totalButtons)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        Debug.Log("<color=green><b>MAZE COMPLETE:</b></color> Se han activado los 3 botones.");
        onAllButtonsPressed?.Invoke();
    }
}