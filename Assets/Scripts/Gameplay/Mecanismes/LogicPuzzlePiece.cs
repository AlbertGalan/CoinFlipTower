using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LogicPuzzlePiece : MonoBehaviour
{
    [Header("Identidad")]
    [SerializeField] private LogicPuzzleSymbol symbol = LogicPuzzleSymbol.None;

    private Rigidbody rb;

    public LogicPuzzleSymbol Symbol => symbol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PrepareForPickup()
    {
        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
        }
    }
}