using UnityEngine;
using UnityEngine.Events;

public class SoldierPuzzleSlot : MonoBehaviour
{
    public enum FeedbackState { Neutral, Correct, Incorrect }

    [Header("Configuración")]
    public DestructibleBlock.BlockColor requiredColor; // El color que este slot espera (ej: Red)
    public Renderer feedbackRenderer;
    public Color neutralColor = Color.white, correctColor = Color.green, incorrectColor = Color.red;

    public UnityEvent OnBlockPlaced;
    public UnityEvent OnBlockRemoved;

    private SoldierPuzzleManager manager;
    private DestructibleBlock currentBlock;
    public DestructibleBlock CurrentBlock => currentBlock;

    private void Awake()
    {
        // Intentamos encontrar el manager en jerarquía local y, si no, en la escena.
        manager = GetComponentInParent<SoldierPuzzleManager>();
        if (manager == null)
        {
            manager = FindFirstObjectByType<SoldierPuzzleManager>();
        }

        if (manager == null)
        {
            Debug.LogWarning($"[SoldierPuzzleSlot] No se encontró SoldierPuzzleManager para el slot {gameObject.name}. No podrá notificar cambios.");
        }
        ApplyFeedbackColor(FeedbackState.Neutral);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentBlock != null) return;

        DestructibleBlock block = other.GetComponent<DestructibleBlock>() ?? other.GetComponentInParent<DestructibleBlock>();

        if (block != null)
        {
            currentBlock = block;
            block.isPlaced = true;
            
            Rigidbody rb = block.GetComponent<Rigidbody>();
            if(rb != null) rb.linearVelocity = Vector3.zero;

            // --- LÓGICA DE COLOR AUTO-SUFICIENTE ---
            if (currentBlock.blockColor == requiredColor)
                ApplyFeedbackColor(FeedbackState.Correct);
            else
                ApplyFeedbackColor(FeedbackState.Incorrect);
            // ----------------------------------------

            OnBlockPlaced.Invoke();
            if (manager != null) manager.NotifySlotChanged();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DestructibleBlock block = other.GetComponent<DestructibleBlock>() ?? other.GetComponentInParent<DestructibleBlock>();
        
        if (block != null && block == currentBlock)
        {
            block.isPlaced = false;
            currentBlock = null;
            
            // Al salir, siempre volvemos a BLANCO
            ApplyFeedbackColor(FeedbackState.Neutral);
            
            if (manager != null) manager.NotifySlotChanged();
            OnBlockRemoved.Invoke();
        }
    }

    public void ApplyFeedbackColor(FeedbackState state)
    {
        if (feedbackRenderer == null) return;

        Color targetColor = neutralColor;
        if (state == FeedbackState.Correct) targetColor = correctColor;
        else if (state == FeedbackState.Incorrect) targetColor = incorrectColor;

        // Aplicación directa de color
        feedbackRenderer.material.color = targetColor;
        
        Debug.Log($"<color=yellow>SLOT {gameObject.name}:</color> Cambiado a {state}");
    }
}