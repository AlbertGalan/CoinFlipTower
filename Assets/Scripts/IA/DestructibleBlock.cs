using UnityEngine;
using System.Collections;

public class DestructibleBlock : MonoBehaviour
{
    public enum BlockColor { None, Blue, Red }

    [Header("Configuración")]
    public float health = 100f;
    public BlockColor blockColor = BlockColor.None;
    
    [HideInInspector] public bool isPlaced = false; // Nova bandera de seguretat

    private Renderer blockRenderer;
    private Color originalColor;

    void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        if (blockRenderer != null) originalColor = blockRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        // SI esta a un slot es invulnerable
        if (isPlaced || health <= 0) return;

        health -= amount;
        StartCoroutine(DamageEffect());

        if (health <= 0) DestroyBlock();
    }

    void DestroyBlock()
    {
        Destroy(gameObject);
    }

    IEnumerator DamageEffect()
    {
        if (blockRenderer == null) yield break;
        blockRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        blockRenderer.material.color = originalColor;
    }
}