using UnityEngine;

public class SimonFloorTile : MonoBehaviour
{
    public SimonDiceManager.SimonColor colorDeEstaCasilla;
    private SimonDiceManager manager;

    void Start()
    {
        manager = FindFirstObjectByType<SimonDiceManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Asegúrate de que tu personaje tenga el tag "Player"
        {
            manager.CasillaPisada(colorDeEstaCasilla);
        }
    }
}