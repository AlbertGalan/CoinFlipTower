using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SimpleNpcAnimator : MonoBehaviour
{
    [Header("Configuración de Animación")]
    [Tooltip("El nombre exacto del Trigger en tu Animator Controller")]
    public string animationTriggerName;

    [Tooltip("Si es true, la animación se activará al iniciar. Si es false, tendrás que activarla externamente.")]
    public bool playOnStart = true;

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (playOnStart && !string.IsNullOrEmpty(animationTriggerName))
        {
            TriggerAnimation();
        }
    }

    /// <summary>
    /// Método público por si quieres activarlo desde otro script o evento.
    /// </summary>
    public void TriggerAnimation()
    {
        if (anim != null)
        {
            anim.SetTrigger(animationTriggerName);
            Debug.Log($"<color=white>NPC {gameObject.name}:</color> Ejecutando trigger '{animationTriggerName}'");
        }
    }
}