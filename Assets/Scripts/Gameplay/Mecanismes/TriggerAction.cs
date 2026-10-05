using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TriggerAction : MonoBehaviour
{
    [System.Serializable]
    public class AnimatorParameter
    {
        public Animator animator;
        public string parameterName;
        public AnimatorControllerParameterType parameterType;
        public bool boolValue;
        public int intValue;
        public float floatValue;
    }

    [System.Serializable]
    public class ComponentToggle
    {
        [Tooltip("Nom del component (ex: 'MoveCharacter', 'Rigidbody', etc.)")]
        public string componentName;
        [Tooltip("Activar (true) o desactivar (false) el component")]
        public bool enableComponent;
        [Tooltip("Buscar també en objectes fills (per exemple, una càmera filla amb un script)")]
        public bool searchInChildren = false;
    }

    [Header("Trigger Detection")]
    public List<string> targetTags = new List<string> { "Player", "Pushable" };

    [Header("Animator Parameters on Enter")]
    public List<AnimatorParameter> enterAnimatorParameters = new List<AnimatorParameter>();

    [Header("Animator Parameters on Exit")]
    public List<AnimatorParameter> exitAnimatorParameters = new List<AnimatorParameter>();

    [Header("Component Toggles on Enter")]
    public List<ComponentToggle> enterComponentToggles = new List<ComponentToggle>();

    [Header("Component Toggles on Exit")]
    public List<ComponentToggle> exitComponentToggles = new List<ComponentToggle>();

    [Header("Events")]
    public UnityEvent OnTriggerEntered;
    public UnityEvent OnTriggerExited;

    [Header("Audio on Enter")]
    [Tooltip("Clip de so a reproduir al entrar")]
    public AudioClip enterAudioClip;
    [Range(0f, 1f)]
    public float audioVolume = 1f;

    [Header("Destruction")]
    public bool destroyOnEnter = false;
    public float destroyDelayEnter = 0f;
    public bool destroyOnExit = false;
    public float destroyDelay = 0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTargetTag(other.tag))
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;

        // Cambios de parámetros al entrar
        foreach (AnimatorParameter param in enterAnimatorParameters)
        {
            if (param.animator != null) SetAnimatorParameter(param.animator, param);
        }

        ApplyComponentToggles(other.gameObject, enterComponentToggles);

        // --- REPRODUCCIÓN DE AUDIO (MODIFICADO) ---
        if (enterAudioClip != null)
        {
            // PlayClipAtPoint permite que el audio siga sonando si el trigger se destruye
            AudioSource.PlayClipAtPoint(enterAudioClip, transform.position, audioVolume);
        }

        OnTriggerEntered.Invoke();
        Debug.Log($"Trigger entered: {gameObject.name}");

        if (destroyOnEnter)
        {
            if (destroyDelayEnter > 0f)
                Destroy(gameObject, destroyDelayEnter);
            else
                Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTargetTag(other.tag))
            return;

        // Resetear el flag solo si no se destruye al entrar
        if (hasTriggered && !destroyOnEnter)
            hasTriggered = false;

        foreach (AnimatorParameter param in exitAnimatorParameters)
        {
            if (param.animator != null) SetAnimatorParameter(param.animator, param);
        }

        ApplyComponentToggles(other.gameObject, exitComponentToggles);

        OnTriggerExited.Invoke();
        Debug.Log($"Trigger exited: {gameObject.name}");

        if (destroyOnExit)
        {
            if (destroyDelay > 0f)
                Destroy(gameObject, destroyDelay);
            else
                Destroy(gameObject);
        }
    }

    private bool IsTargetTag(string tag)
    {
        return targetTags.Contains(tag);
    }

    private void SetAnimatorParameter(Animator animator, AnimatorParameter param)
    {
        switch (param.parameterType)
        {
            case AnimatorControllerParameterType.Bool:
                animator.SetBool(param.parameterName, param.boolValue);
                break;
            case AnimatorControllerParameterType.Int:
                animator.SetInteger(param.parameterName, param.intValue);
                break;
            case AnimatorControllerParameterType.Float:
                animator.SetFloat(param.parameterName, param.floatValue);
                break;
            case AnimatorControllerParameterType.Trigger:
                animator.SetTrigger(param.parameterName);
                break;
        }
    }

    private void ApplyComponentToggles(GameObject target, List<ComponentToggle> toggles)
    {
        foreach (ComponentToggle toggle in toggles)
        {
            if (string.IsNullOrEmpty(toggle.componentName)) continue;

            System.Type componentType = System.Type.GetType(toggle.componentName);
            if (componentType == null) continue;

            Component component = toggle.searchInChildren 
                ? target.GetComponentInChildren(componentType)
                : target.GetComponent(componentType);

            if (component != null && component is Behaviour)
            {
                ((Behaviour)component).enabled = toggle.enableComponent;
            }
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}