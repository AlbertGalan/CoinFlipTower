using UnityEngine;
using System.Collections;

public class EasterEggBox : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Vector3 rotationOffset = new Vector3(-45, 0, 0); 
    public float transitionSpeed = 2f;
    
    [Header("Referencias y Audio")]
    public AudioClip easterEggClip; 
    [Range(0f, 1f)] public float volume = 0.8f;
    public GameObject scorePickup; 

    private Quaternion originalRotation;
    private Quaternion openRotation;
    private bool isExecuting = false;

    void Start()
    {
        originalRotation = transform.localRotation;
        openRotation = originalRotation * Quaternion.Euler(rotationOffset);
        
        if (scorePickup != null) scorePickup.SetActive(false);
    }

    public void ActivateEasterEgg()
    {
        // Si el script está desactivado, esta función ni siquiera debería llegar a llamarse,
        // pero añadimos el check por seguridad extra.
        if (isExecuting) return; 

        StartCoroutine(EasterEggSequence());
    }

    IEnumerator EasterEggSequence()
    {
        isExecuting = true;

        // 1. ABRIR
        float elapsed = 0;
        while (elapsed < 1f)
        {
            transform.localRotation = Quaternion.Slerp(originalRotation, openRotation, elapsed);
            elapsed += Time.deltaTime * transitionSpeed;
            yield return null;
        }
        transform.localRotation = openRotation;

        // 2. AUDIO
        if (easterEggClip != null)
        {
            AudioSource.PlayClipAtPoint(easterEggClip, transform.position, volume);
            yield return new WaitForSeconds(easterEggClip.length);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // 3. PREMIO
        if (scorePickup != null)
        {
            scorePickup.SetActive(true);
        }

        // 4. CERRAR
        elapsed = 0;
        while (elapsed < 1f)
        {
            transform.localRotation = Quaternion.Slerp(openRotation, originalRotation, elapsed);
            elapsed += Time.deltaTime * transitionSpeed;
            yield return null;
        }
        transform.localRotation = originalRotation;

        // --- EL TOQUE FINAL ---
        Debug.Log("Easter Egg completado. Desactivando script.");
        
        // Cambiamos la capa para que el Raycast/Outline lo ignore para siempre
        gameObject.layer = LayerMask.NameToLayer("Default");

        // Desactivamos el componente
        this.enabled = false; 
    }
}