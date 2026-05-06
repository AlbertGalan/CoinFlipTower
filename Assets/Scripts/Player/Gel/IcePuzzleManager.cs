using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class IcePuzzleManager : MonoBehaviour
{
    public List<IceContainer> tubos;
    public UnityEvent onPuzzleComplete;

    private void OnEnable()
    {
        foreach (var tubo in tubos)
        {
            tubo.onBlockEntered.AddListener(CheckPuzzleState);
        }
    }

    void CheckPuzzleState()
    {
        int llenos = 0;
        foreach (var tubo in tubos)
        {
            if (tubo.estaLleno) llenos++;
        }

        if (llenos >= tubos.Count)
        {
            Debug.Log("¡Puzzle completado!");
            onPuzzleComplete.Invoke();
        }
    }
}