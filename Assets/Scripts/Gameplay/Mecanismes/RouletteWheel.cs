using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RouletteWheel : MonoBehaviour
{
    public enum ElementType
    {
        Fuego,
        Tierra,
        Hielo
    }

    [System.Serializable]
    public class RouletteResultEvent : UnityEvent<ElementType> { }

    [Header("Sectors (percentage)")]
    [Range(0f, 100f)] public float fuegoWeight = 33f;
    [Range(0f, 100f)] public float tierraWeight = 33f;
    [Range(0f, 100f)] public float hieloWeight = 34f;

    [Header("Spin")]
    [Tooltip("Axis in local space. For a wall roulette, this is usually Forward or Up depending on mesh orientation.")]
    public Vector3 localSpinAxis = Vector3.forward;
    [Tooltip("Extra full turns before stopping.")]
    [Min(1)] public int minFullTurns = 4;
    [Min(1)] public int maxFullTurns = 7;
    [Tooltip("Spin duration in seconds.")]
    [Min(0.1f)] public float spinDuration = 2.8f;

    [Header("Alignment")]
    [Tooltip("Transform of the arrow tip. If assigned, the script reads the pointer direction from scene geometry.")]
    public Transform pointerTip;
    [Tooltip("Direction in roulette local space that represents 0 degrees (start of Fuego sector).")]
    public Vector3 zeroAngleLocalDirection = Vector3.up;
    [Tooltip("Angular size covered by the pointer, in degrees.")]
    [Range(0.1f, 90f)] public float pointerArcWidthDegrees = 8f;
    [Tooltip("Used only if pointerTip is not assigned.")]
    [Range(0f, 360f)] public float fallbackPointerAngleDegrees = 0f;

    [Header("Debug")]
    public bool logResult = true;

    [Header("Events")]
    public UnityEvent OnSpinStart;
    public RouletteResultEvent OnSpinEnd;

    public bool IsSpinning => isSpinning;
    public ElementType LastResult { get; private set; }

    private bool isSpinning;

    public void Spin()
    {
        if (isSpinning)
            return;

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        OnSpinStart?.Invoke();

        ElementType pickedResult = PickWeightedResult();
        float targetPointerAngleCW = GetSafeAngleInsideSectorCW(pickedResult);
        float currentPointerAngleCW = GetPointerCenterAngleCW();
        int fullTurns = Random.Range(minFullTurns, maxFullTurns + 1);
        Vector3 axis = localSpinAxis.sqrMagnitude > 0.0001f ? localSpinAxis.normalized : Vector3.forward;
        Quaternion baseRotation = transform.localRotation;

        // Clockwise amount to move from current pointer sector to target sector.
        float clockwiseDelta = Mathf.Repeat(targetPointerAngleCW - currentPointerAngleCW, 360f) + fullTurns * 360f;
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);

            // Ease-out cubic for a roulette-like deceleration.
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float currentClockwise = Mathf.LerpUnclamped(0f, clockwiseDelta, eased);
            transform.localRotation = baseRotation * Quaternion.AngleAxis(-currentClockwise, axis);

            yield return null;
        }

        // Final snap to exact destination.
        transform.localRotation = baseRotation * Quaternion.AngleAxis(-clockwiseDelta, axis);

        LastResult = GetResultFromPointerCoverage();
        isSpinning = false;
        OnSpinEnd?.Invoke(LastResult);

        if (logResult)
        {
            Debug.Log($"[RouletteWheel] Result: {LastResult} (picked: {pickedResult})");
        }
    }

    private ElementType PickWeightedResult()
    {
        float total = Mathf.Max(0f, fuegoWeight) + Mathf.Max(0f, tierraWeight) + Mathf.Max(0f, hieloWeight);
        if (total <= 0f)
        {
            return ElementType.Fuego;
        }

        float roll = Random.Range(0f, total);
        float acc = Mathf.Max(0f, fuegoWeight);
        if (roll <= acc) return ElementType.Fuego;

        acc += Mathf.Max(0f, tierraWeight);
        if (roll <= acc) return ElementType.Tierra;

        return ElementType.Hielo;
    }

    private float GetSafeAngleInsideSectorCW(ElementType result)
    {
        GetSectorBoundsCW(result, out float min, out float max);

        float padding = pointerArcWidthDegrees * 0.5f + 1f;
        float minSafe = min + padding;
        float maxSafe = max - padding;

        if (maxSafe <= minSafe)
        {
            return Mathf.Repeat((min + max) * 0.5f, 360f);
        }

        return Random.Range(minSafe, maxSafe);
    }

    private float GetPointerCenterAngleCW()
    {
        if (pointerTip == null)
        {
            return Mathf.Repeat(fallbackPointerAngleDegrees, 360f);
        }

        Vector3 axisWorld = transform.TransformDirection(localSpinAxis.sqrMagnitude > 0.0001f ? localSpinAxis.normalized : Vector3.forward);
        Vector3 zeroWorld = transform.TransformDirection(zeroAngleLocalDirection.sqrMagnitude > 0.0001f ? zeroAngleLocalDirection.normalized : Vector3.up);
        zeroWorld = Vector3.ProjectOnPlane(zeroWorld, axisWorld).normalized;

        Vector3 toPointer = pointerTip.position - transform.position;
        Vector3 pointerOnPlane = Vector3.ProjectOnPlane(toPointer, axisWorld);
        if (pointerOnPlane.sqrMagnitude <= 0.000001f || zeroWorld.sqrMagnitude <= 0.000001f)
        {
            return Mathf.Repeat(fallbackPointerAngleDegrees, 360f);
        }

        float signed = Vector3.SignedAngle(zeroWorld, pointerOnPlane.normalized, axisWorld);
        return Mathf.Repeat(-signed, 360f);
    }

    private ElementType GetResultFromPointerCoverage()
    {
        int samples = 9;
        float halfArc = pointerArcWidthDegrees * 0.5f;
        float center = GetPointerCenterAngleCW();

        int fireCount = 0;
        int earthCount = 0;
        int iceCount = 0;

        for (int i = 0; i < samples; i++)
        {
            float t = samples == 1 ? 0f : i / (samples - 1f);
            float a = Mathf.Repeat((center - halfArc) + t * (pointerArcWidthDegrees), 360f);
            ElementType sampleResult = GetResultAtAngleCW(a);
            switch (sampleResult)
            {
                case ElementType.Fuego:
                    fireCount++;
                    break;
                case ElementType.Tierra:
                    earthCount++;
                    break;
                default:
                    iceCount++;
                    break;
            }
        }

        if (fireCount >= earthCount && fireCount >= iceCount) return ElementType.Fuego;
        if (earthCount >= fireCount && earthCount >= iceCount) return ElementType.Tierra;
        return ElementType.Hielo;
    }

    private ElementType GetResultAtAngleCW(float angleCW)
    {
        GetSectorBoundsCW(ElementType.Fuego, out float fMin, out float fMax);
        GetSectorBoundsCW(ElementType.Tierra, out float tMin, out float tMax);

        if (IsAngleInRangeCW(angleCW, fMin, fMax)) return ElementType.Fuego;
        if (IsAngleInRangeCW(angleCW, tMin, tMax)) return ElementType.Tierra;
        return ElementType.Hielo;
    }

    private bool IsAngleInRangeCW(float angleCW, float min, float max)
    {
        angleCW = Mathf.Repeat(angleCW, 360f);
        min = Mathf.Repeat(min, 360f);
        max = Mathf.Repeat(max, 360f);

        if (min <= max)
            return angleCW >= min && angleCW < max;

        return angleCW >= min || angleCW < max;
    }

    private void GetSectorBoundsCW(ElementType type, out float min, out float max)
    {
        float total = Mathf.Max(0f, fuegoWeight) + Mathf.Max(0f, tierraWeight) + Mathf.Max(0f, hieloWeight);
        if (total <= 0f)
        {
            min = 0f;
            max = 360f;
            return;
        }

        float fuegoDeg = 360f * Mathf.Max(0f, fuegoWeight) / total;
        float tierraDeg = 360f * Mathf.Max(0f, tierraWeight) / total;
        float hieloDeg = 360f * Mathf.Max(0f, hieloWeight) / total;

        switch (type)
        {
            case ElementType.Fuego:
                min = 0f;
                max = fuegoDeg;
                break;
            case ElementType.Tierra:
                min = fuegoDeg;
                max = fuegoDeg + tierraDeg;
                break;
            default:
                min = fuegoDeg + tierraDeg;
                max = fuegoDeg + tierraDeg + hieloDeg;
                break;
        }
    }

    private void OnValidate()
    {
        if (maxFullTurns < minFullTurns)
        {
            maxFullTurns = minFullTurns;
        }

        if (localSpinAxis.sqrMagnitude <= 0.0001f)
        {
            localSpinAxis = Vector3.forward;
        }

        if (zeroAngleLocalDirection.sqrMagnitude <= 0.0001f)
        {
            zeroAngleLocalDirection = Vector3.up;
        }

        if (pointerArcWidthDegrees < 0.1f)
        {
            pointerArcWidthDegrees = 0.1f;
        }
    }
}