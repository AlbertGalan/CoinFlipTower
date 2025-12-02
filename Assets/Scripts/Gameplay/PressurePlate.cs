using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// PressurePlate: simple trigger-based pressure plate.
/// - Place a GameObject with a BoxCollider (IsTrigger = true) and this component.
/// - It detects Rigidbody objects whose GameObject layer is included in `detectionMask`.
/// - It can use a mass threshold (either per-object or total mass) to decide activation.
/// - Exposes UnityEvents `onPressed` and `onReleased` for designers to hook logic (doors, switches, etc.).
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Which layers will be considered for activation")]
    public LayerMask detectionMask = ~0;

    [Tooltip("If useTotalMass == false, any object with mass >= requiredMass will activate. If true, sum of masses must be >= requiredMass.")]
    public float requiredMass = 0.1f;

    [Tooltip("When true, the plate sums the mass of all overlapping rigidbodies and compares to requiredMass.")]
    public bool useTotalMass = false;

    [Header("Events")]
    public UnityEvent onPressed;
    public UnityEvent onReleased;
    [Header("Debug")]
    [Tooltip("Enable debug logs when the plate is pressed/released")]
    public bool debugLogs = false;

    // internal set of overlapping rigidbodies
    private readonly HashSet<Rigidbody> overlapping = new HashSet<Rigidbody>();
    private bool isPressed = false;

    void Reset()
    {
        // ensure there's a trigger collider on the same GameObject
        var col = GetComponent<Collider>();
        if (col == null)
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
        else
            col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (debugLogs)
        {
            Debug.Log($"PressurePlate '{name}': OnTriggerEnter -> '{other.name}' (layer={other.gameObject.layer})");
        }

        if (!LayerInMask(other.gameObject.layer, detectionMask))
        {
            if (debugLogs) Debug.Log($"PressurePlate '{name}': ignored layer {other.gameObject.layer} (not in detectionMask)");
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            if (debugLogs) Debug.Log($"PressurePlate '{name}': collider '{other.name}' has no attached Rigidbody (ignored)");
            return;
        }

        overlapping.Add(rb);
        if (debugLogs) Debug.Log($"PressurePlate '{name}': added Rigidbody '{rb.name}' (mass={rb.mass:F2}). Overlapping count = {overlapping.Count}");
        Evaluate();
    }

    void OnTriggerExit(Collider other)
    {
        if (debugLogs)
        {
            Debug.Log($"PressurePlate '{name}': OnTriggerExit -> '{other.name}' (layer={other.gameObject.layer})");
        }

        if (!LayerInMask(other.gameObject.layer, detectionMask))
        {
            if (debugLogs) Debug.Log($"PressurePlate '{name}': exit ignored layer {other.gameObject.layer}");
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            if (debugLogs) Debug.Log($"PressurePlate '{name}': exit collider '{other.name}' has no attached Rigidbody (ignored)");
            return;
        }

        overlapping.Remove(rb);
        if (debugLogs) Debug.Log($"PressurePlate '{name}': removed Rigidbody '{rb.name}'. Overlapping count = {overlapping.Count}");
        Evaluate();
    }

    private void Evaluate()
    {
        bool pressed = false;

        if (useTotalMass)
        {
            float total = 0f;
            foreach (var rb in overlapping)
            {
                if (rb == null) continue;
                total += rb.mass;
            }
            pressed = total >= requiredMass;
        }
        else
        {
            foreach (var rb in overlapping)
            {
                if (rb == null) continue;
                if (rb.mass >= requiredMass)
                {
                    pressed = true;
                    break;
                }
            }
        }

        if (pressed != isPressed)
        {
            isPressed = pressed;
            if (isPressed)
            {
                if (debugLogs)
                {
                    if (useTotalMass)
                    {
                        float total = 0f;
                        foreach (var rb in overlapping) if (rb != null) total += rb.mass;
                        Debug.Log($"PressurePlate '{name}' PRESSED (totalMass={total:F2} >= required={requiredMass:F2})");
                    }
                    else
                    {
                        Rigidbody trigger = null;
                        foreach (var rb in overlapping) if (rb != null && rb.mass >= requiredMass) { trigger = rb; break; }
                        Debug.Log($"PressurePlate '{name}' PRESSED by '{(trigger!=null?trigger.name:"(unknown)")}' (mass >= {requiredMass:F2})");
                    }
                }

                onPressed?.Invoke();
            }
            else
            {
                if (debugLogs)
                {
                    Debug.Log($"PressurePlate '{name}' RELEASED");
                }

                onReleased?.Invoke();
            }
        }
    }

    private static bool LayerInMask(int layer, LayerMask mask)
    {
        return ((1 << layer) & mask) != 0;
    }

    public bool IsPressed() => isPressed;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isPressed ? Color.green : Color.red;
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else
            {
                Gizmos.DrawWireSphere(col.bounds.center, Mathf.Max(col.bounds.extents.x, col.bounds.extents.z));
            }
        }
    }
#endif
}
