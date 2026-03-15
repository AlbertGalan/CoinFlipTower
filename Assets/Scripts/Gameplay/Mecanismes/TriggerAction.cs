using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    [Tooltip("Tags que activarán el trigger")]
    public List<string> targetTags = new List<string> { "Player", "Pushable" };

    [Header("Animator Parameters on Enter")]
    [Tooltip("Parámetros del animator a cambiar al entrar")]
    public List<AnimatorParameter> enterAnimatorParameters = new List<AnimatorParameter>();

    [Header("Animator Parameters on Exit")]
    [Tooltip("Parámetros del animator a cambiar al salir")]
    public List<AnimatorParameter> exitAnimatorParameters = new List<AnimatorParameter>();

    [Header("Component Toggles on Enter")]
    [Tooltip("Components a activar/desactivar en l'objecte que entra")]
    public List<ComponentToggle> enterComponentToggles = new List<ComponentToggle>();

    [Header("Component Toggles on Exit")]
    [Tooltip("Components a activar/desactivar en l'objecte que surt")]
    public List<ComponentToggle> exitComponentToggles = new List<ComponentToggle>();

    [Header("Events")]
    public UnityEvent OnTriggerEntered;
    public UnityEvent OnTriggerExited;

    [Header("Audio on Enter")]
    [Tooltip("AudioSource a reproducir al entrar en el trigger")]
    public AudioSource enterAudioSource;

    [Header("Destruction")]
    [Tooltip("¿Destruir el trigger al entrar o al salir?")]

    public bool destroyOnEnter = false;
    [Tooltip("Delay en segundos antes de destruir al entrar")]
    public float destroyDelayEnter = 0f;

    public bool destroyOnExit = false;
    [Tooltip("Delay en segundos antes de destruir")]
    public float destroyDelay = 0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTargetTag(other.tag))
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;

        // Aplicar cambios de parámetros al entrar
        foreach (AnimatorParameter param in enterAnimatorParameters)
        {
            if (param.animator != null)
            {
                SetAnimatorParameter(param.animator, param);
            }
        }

        // Activar/desactivar components de l'objecte que entra
        ApplyComponentToggles(other.gameObject, enterComponentToggles);

        // Reproducir audio al entrar
        if (enterAudioSource != null)
        {
            enterAudioSource.Play();
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

        // Aplicar cambios de parámetros al salir
        foreach (AnimatorParameter param in exitAnimatorParameters)
        {
            if (param.animator != null)
            {
                SetAnimatorParameter(param.animator, param);
            }
        }

        // Activar/desactivar components de l'objecte que surt
        ApplyComponentToggles(other.gameObject, exitComponentToggles);

        OnTriggerExited.Invoke();
        Debug.Log($"Trigger exited: {gameObject.name}");

        // Destruir el trigger si está configurado
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
                Debug.Log($"Set Bool '{param.parameterName}' to {param.boolValue} on {animator.gameObject.name}");
                break;
            case AnimatorControllerParameterType.Int:
                animator.SetInteger(param.parameterName, param.intValue);
                Debug.Log($"Set Int '{param.parameterName}' to {param.intValue} on {animator.gameObject.name}");
                break;
            case AnimatorControllerParameterType.Float:
                animator.SetFloat(param.parameterName, param.floatValue);
                Debug.Log($"Set Float '{param.parameterName}' to {param.floatValue} on {animator.gameObject.name}");
                break;
            case AnimatorControllerParameterType.Trigger:
                animator.SetTrigger(param.parameterName);
                Debug.Log($"Triggered '{param.parameterName}' on {animator.gameObject.name}");
                break;
        }
    }

    private void ApplyComponentToggles(GameObject target, List<ComponentToggle> toggles)
    {
        foreach (ComponentToggle toggle in toggles)
        {
            if (string.IsNullOrEmpty(toggle.componentName))
                continue;

            // Buscar el component per nom (en l'objecte o en fills segons configuració)
            System.Type componentType = System.Type.GetType(toggle.componentName);
            if (componentType == null)
            {
                Debug.LogWarning($"Component type '{toggle.componentName}' not found");
                continue;
            }

            Component component = toggle.searchInChildren 
                ? target.GetComponentInChildren(componentType)
                : target.GetComponent(componentType);

            if (component != null && component is Behaviour)
            {
                ((Behaviour)component).enabled = toggle.enableComponent;
                Debug.Log($"Component '{toggle.componentName}' set to {toggle.enableComponent} on {component.gameObject.name}");
            }
            else if (component != null)
            {
                Debug.LogWarning($"Component '{toggle.componentName}' on {target.name} is not a Behaviour and cannot be enabled/disabled.");
            }
            else
            {
                string searchScope = toggle.searchInChildren ? "or children" : "";
                Debug.LogWarning($"Component '{toggle.componentName}' not found on {target.name} {searchScope}");
            }
        }
    }

    // Métodos públicos para resetear si es necesario
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
