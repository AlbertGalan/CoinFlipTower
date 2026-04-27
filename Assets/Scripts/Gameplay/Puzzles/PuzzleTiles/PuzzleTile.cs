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
            // 1. Si el puzzle ya terminó, no hacemos nada más
            if (manager.IsComplete) return;

            // 2. Si la baldosa está apagada, la encendemos
            if (currentState == TileState.Off)
            {
                SetState(TileState.On);
                manager.OnTileActivated();
            }
            // 3. Si ya estaba encendida, solo fallamos SI NO es la última que faltaba
            // (Esta comprobación la hace el manager internamente ahora)
            else if (currentState == TileState.On)
            {
                // Solo activamos lava si el manager confirma que no hemos ganado
                SetState(TileState.Lava);
                manager.TriggerFailure(other.gameObject);
            }
        }
    }
}