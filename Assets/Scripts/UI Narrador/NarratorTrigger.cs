using UnityEngine;
using System.Collections.Generic;

public class NarratorTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [TextArea(3, 5)]
    public List<string> lines;
    public Sprite customNarratorSprite; // Opcional
    public bool destroyAfterUse = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (NarratorUI.Instance != null && lines.Count > 0)
            {
                NarratorUI.Instance.TriggerDialogue(lines, customNarratorSprite);
                
                if (destroyAfterUse)
                    Destroy(gameObject);
            }
        }
    }
}