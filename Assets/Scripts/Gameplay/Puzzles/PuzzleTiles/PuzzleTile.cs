using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    public enum TileState { Off, On, Lava }
    public TileState currentState = TileState.Off;

    [Header("Materiales")]
    public Material matOff;
    public Material matOn;
    public Material matLava;

    private MeshRenderer meshRenderer;
    private TilePuzzleManager manager;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        manager = GetComponentInParent<TilePuzzleManager>();
        
        // Failsafe por si el manager no está en el padre directo
        if (manager == null) manager = FindFirstObjectByType<TilePuzzleManager>();
        
        SetState(TileState.Off);
    }

    public void SetState(TileState newState)
    {
        currentState = newState;
        if (meshRenderer == null) return;

        switch (newState)
        {
            case TileState.Off: meshRenderer.material = matOff; break;
            case TileState.On: meshRenderer.material = matOn; break;
            case TileState.Lava: meshRenderer.material = matLava; break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Si el puzzle ya terminó, las baldosas son seguras
            if (manager != null && manager.IsComplete) return;

            // 2. Si la baldosa está apagada, la activamos
            if (currentState == TileState.Off)
            {
                SetState(TileState.On);
                manager.OnTileActivated();
            }
            // 3. Si ya estaba encendida, el jugador ha pisado donde no debía
            else if (currentState == TileState.On)
            {
                // Cambiamos visualmente a lava para dar feedback del error
                SetState(TileState.Lava);
                
                // Avisamos al manager para que reinicie el puzzle y haga el respawn
                manager.TriggerFailure(other.gameObject);
            }
        }
    }
}