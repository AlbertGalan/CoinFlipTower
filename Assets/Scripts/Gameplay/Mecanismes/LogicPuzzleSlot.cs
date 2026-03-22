using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LogicPuzzleSlot : MonoBehaviour
{
    public enum FeedbackState
    {
        Neutral,
        Correct,
        Incorrect
    }

    [Header("Configuración")]
    [Tooltip("ID opcional para identificar el slot")]
    public string slotId;

    [Header("Feedback visual (opcional)")]
    public Renderer feedbackRenderer;
    public Color neutralColor = Color.white;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;

    [Header("Eventos")]
    public UnityEvent OnPiecePlaced;
    public UnityEvent OnPieceRemoved;

    private LogicPuzzleManager manager;
    private LogicPuzzlePiece currentPiece;
    private FeedbackState feedbackState = FeedbackState.Neutral;
    private MaterialPropertyBlock propertyBlock;
    private Collider slotCollider;

    public LogicPuzzlePiece CurrentPiece => currentPiece;

    private void Awake()
    {
        manager = GetComponentInParent<LogicPuzzleManager>();
        slotCollider = GetComponent<Collider>();
        propertyBlock = new MaterialPropertyBlock();
        ApplyFeedbackColor(feedbackState);
    }

    private void FixedUpdate()
    {
        // Chequeo manual para detectar piezas dentro del trigger (por si el movimiento del Joint no dispara triggers bien)
        CheckForPiecesInZone();
    }

    private void CheckForPiecesInZone()
    {
        if (slotCollider == null || !slotCollider.isTrigger) return;

        // Usar Physics.OverlapBox para detectar todos los colliders en la zona
        Collider[] colliders = Physics.OverlapBox(
            slotCollider.bounds.center,
            slotCollider.bounds.extents,
            transform.rotation
        );

        LogicPuzzlePiece foundPiece = null;

        foreach (Collider col in colliders)
        {
            if (col == slotCollider) continue; // Ignorar el propio trigger
            
            LogicPuzzlePiece piece = col.GetComponent<LogicPuzzlePiece>();
            if (piece != null)
            {
                foundPiece = piece;
                break;
            }
            
            // Alternativa: si el collider está attached a un Rigidbody que tiene LogicPuzzlePiece
            LogicPuzzlePiece pieceOnRb = col.attachedRigidbody?.GetComponent<LogicPuzzlePiece>();
            if (pieceOnRb != null)
            {
                foundPiece = pieceOnRb;
                break;
            }
        }

        // Si encontramos una pieza y era diferente, notificar
        if (foundPiece != currentPiece)
        {
            if (foundPiece != null)
            {
                currentPiece = foundPiece;
                Debug.Log($"[Slot {slotId}] Pieza detectada por OverlapBox: {foundPiece.gameObject.name}");
                OnPiecePlaced.Invoke();

                if (manager != null)
                {
                    manager.NotifySlotChanged(this);
                }
            }
            else if (currentPiece != null)
            {
                Debug.Log($"[Slot {slotId}] Pieza removida por OverlapBox: {currentPiece.gameObject.name}");
                currentPiece = null;
                OnPieceRemoved.Invoke();

                if (manager != null)
                {
                    manager.NotifySlotChanged(this);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Slot {slotId}] *** TRIGGER CALLED *** - objeto: {other.gameObject.name}, layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        
        LogicPuzzlePiece piece = other.GetComponent<LogicPuzzlePiece>();
        if (piece == null)
        {
            Debug.Log($"[Slot] {other.gameObject.name} no tiene LogicPuzzlePiece (no es un bloque válido)");
            return;
        }
        
        Debug.Log($"[Slot] Bloque detectado: {piece.gameObject.name} con símbolo: {piece.Symbol}");
        
        if (currentPiece != null && currentPiece != piece)
        {
            Debug.Log($"[Slot] Ya hay una pieza: {currentPiece.gameObject.name}, ignorando la nueva");
            return;
        }

        currentPiece = piece;
        Debug.Log($"[Slot {slotId}] Pieza colocada: {piece.gameObject.name}");
        OnPiecePlaced.Invoke();

        if (manager != null)
        {
            manager.NotifySlotChanged(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LogicPuzzlePiece piece = other.GetComponent<LogicPuzzlePiece>();
        if (piece == null) return;
        if (currentPiece != piece)
        {
            Debug.Log($"[Slot] OnTriggerExit: pieza {piece.gameObject.name} no coincide con actual");
            return;
        }

        Debug.Log($"[Slot {slotId}] Pieza removida: {piece.gameObject.name}");
        currentPiece = null;
        OnPieceRemoved.Invoke();

        if (manager != null)
        {
            manager.NotifySlotChanged(this);
        }
    }

    public void SetFeedbackState(FeedbackState state)
    {
        if (feedbackState == state) return;
        feedbackState = state;
        ApplyFeedbackColor(state);
    }

    private void ApplyFeedbackColor(FeedbackState state)
    {
        if (feedbackRenderer == null)
        {
            Debug.LogWarning($"[Slot {slotId}] feedbackRenderer está vacio, no se puede aplicar color");
            return;
        }

        Color targetColor = neutralColor;
        switch (state)
        {
            case FeedbackState.Correct:
                targetColor = correctColor;
                Debug.Log($"[Slot {slotId}] Cambiando a color CORRECTO: {targetColor}");
                break;
            case FeedbackState.Incorrect:
                targetColor = incorrectColor;
                Debug.Log($"[Slot {slotId}] Cambiando a color INCORRECTO: {targetColor}");
                break;
            case FeedbackState.Neutral:
                Debug.Log($"[Slot {slotId}] Cambiando a color NEUTRO: {targetColor}");
                break;
        }

        // Método directo: cambiar material.color en lugar de PropertyBlock
        feedbackRenderer.material.color = targetColor;
        Debug.Log($"[Slot {slotId}] Color aplicado: {feedbackRenderer.material.color}");
    }
}