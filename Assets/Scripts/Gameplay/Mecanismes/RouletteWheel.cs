using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RouletteWheel : MonoBehaviour
{
    public enum ElementType { Fuego, Tierra, Hielo }

    [Header("Sectors (percentage)")]
    [Range(0f, 100f)] public float fuegoWeight = 33f;
    [Range(0f, 100f)] public float tierraWeight = 33f;
    [Range(0f, 100f)] public float hieloWeight = 34f;

    [Header("Spin")]
    public int minFullTurns = 4;
    public int maxFullTurns = 7;
    public float spinDuration = 2.8f;

    [Header("Alignment")]
    public Transform pointerTip;
    public Vector3 zeroAngleLocalDirection = Vector3.up;
    [Range(0.1f, 90f)] public float pointerArcWidthDegrees = 8f;

    // Variables de estado
    private bool isSpinning;
    private float startAngleX;

    // VALORES PREDETERMINADOS SEGÚN TU EXPLICACIÓN
    // Nota: Si en tu inspector ves 90 positivo, lo dejamos en 90f.
    private const float FIXED_Y = -90f;
    private const float FIXED_Z = 90f; 

    public void Spin()
    {
        if (isSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        
        // Capturamos el X actual del inspector
        startAngleX = transform.localEulerAngles.x;

        ElementType pickedResult = PickWeightedResult();
        float targetPointerAngleCW = GetSafeAngleInsideSectorCW(pickedResult);
        float currentPointerAngleCW = GetPointerCenterAngleCW();
        
        int fullTurns = Random.Range(minFullTurns, maxFullTurns + 1);
        float clockwiseDelta = Mathf.Repeat(targetPointerAngleCW - currentPointerAngleCW, 360f) + (fullTurns * 360f);
        
        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            
            ApplyRotation(eased * clockwiseDelta);
            yield return null;
        }

        ApplyRotation(clockwiseDelta);
        isSpinning = false;
        Debug.Log("Resultado: " + pickedResult);
    }

    private void ApplyRotation(float addedAngle)
    {
        // Calculamos el nuevo valor de X
        float newX = startAngleX - addedAngle;

        // FORZADO DE EJES: 
        // No usamos Quaternion.Euler directamente para evitar que Unity 
        // recalcule los ángulos internos y cambie el -90 por 90.
        transform.localEulerAngles = new Vector3(newX, FIXED_Y, FIXED_Z);
    }

    private float GetPointerCenterAngleCW()
    {
        if (pointerTip == null) return 0f;

        // Calculamos la dirección local hacia la punta de la flecha
        Vector3 directionToPointer = transform.InverseTransformPoint(pointerTip.position);
        directionToPointer.x = 0; // Proyección en plano YZ local
        
        float angle = Vector3.SignedAngle(zeroAngleLocalDirection, directionToPointer, Vector3.right);
        return Mathf.Repeat(-angle, 360f);
    }

    private ElementType PickWeightedResult()
    {
        float total = fuegoWeight + tierraWeight + hieloWeight;
        float roll = Random.Range(0f, total);
        if (roll < fuegoWeight) return ElementType.Fuego;
        if (roll < fuegoWeight + tierraWeight) return ElementType.Tierra;
        return ElementType.Hielo;
    }

    private float GetSafeAngleInsideSectorCW(ElementType result)
    {
        // Ajusta estos rangos según los colores de tu textura
        if (result == ElementType.Fuego) return Random.Range(10f, 110f);
        if (result == ElementType.Tierra) return Random.Range(130f, 230f);
        return Random.Range(250f, 350f);
    }
}