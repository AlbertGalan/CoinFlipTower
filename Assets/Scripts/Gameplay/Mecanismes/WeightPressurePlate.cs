using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public class WeightPressurePlate : MonoBehaviour
{
    [Header("Target Weight Settings")]
    [Tooltip("El peso objetivo que se debe alcanzar para resolver el puzzle")]
    public int targetWeight = 10;

    [Header("Animation")]
    public Animator placa;

    [Header("Events")]
    public UnityEvent OnCorrectWeight;
    public UnityEvent OnIncorrectWeight;

    [Header("TextMeshPro UI (Opcional)")]
    [Tooltip("Texto que muestra el objetivo de la balanza")]
    public TMP_Text targetWeightText;
    [Tooltip("Texto que muestra el peso actual")]
    public TMP_Text currentWeightText;

    [Header("Debug")]
    [Tooltip("Mostrar información de debug en consola")]
    public bool showDebugInfo = true;

    private int currentWeight = 0;
    private bool puzzleSolved = false;
    private List<WeightBlock> blocksOnPlate = new List<WeightBlock>();

    private void Start()
    {
        UpdateDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entra tiene el componente WeightBlock
        WeightBlock block = other.GetComponent<WeightBlock>();
        
        if (block != null && !blocksOnPlate.Contains(block))
        {
            // Añade el bloque a la lista y suma su peso
            blocksOnPlate.Add(block);
            currentWeight += block.GetWeight();

            if (showDebugInfo)
            {
                Debug.Log($"Bloque añadido. Peso: {block.GetWeight()} | Peso total: {currentWeight}/{targetWeight}");
            }

            // Actualiza el estado de la placa
            UpdatePlateState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verifica si el objeto que sale tiene el componente WeightBlock
        WeightBlock block = other.GetComponent<WeightBlock>();
        
        if (block != null && blocksOnPlate.Contains(block))
        {
            // Remueve el bloque de la lista y resta su peso
            blocksOnPlate.Remove(block);
            currentWeight -= block.GetWeight();

            if (showDebugInfo)
            {
                Debug.Log($"Bloque retirado. Peso: {block.GetWeight()} | Peso total: {currentWeight}/{targetWeight}");
            }

            // Si el puzzle estaba resuelto y se retira un bloque, se desactiva
            if (puzzleSolved)
            {
                puzzleSolved = false;
                OnIncorrectWeight.Invoke();
                
                if (showDebugInfo)
                {
                    Debug.Log("Puzzle desactivado - bloque retirado");
                }
            }

            // Actualiza el estado de la placa
            UpdatePlateState();
        }
    }

    private void UpdatePlateState()
    {
        // Actualiza la animación de la placa si está hundida o no
        if (placa != null)
        {
            placa.SetBool("isPressed", blocksOnPlate.Count > 0);
        }

        UpdateDisplay();

        // Verifica si se ha alcanzado el peso objetivo
        if (currentWeight == targetWeight && !puzzleSolved)
        {
            puzzleSolved = true;
            OnCorrectWeight.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log("¡PUZZLE RESUELTO! Peso correcto alcanzado: " + currentWeight);
            }
        }
        else if (currentWeight != targetWeight && puzzleSolved)
        {
            puzzleSolved = false;
            OnIncorrectWeight.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log("Peso incorrecto: " + currentWeight + "/" + targetWeight);
            }
        }
    }

    private void UpdateDisplay()
    {
        if (targetWeightText != null)
        {
            targetWeightText.text = targetWeight.ToString();
        }

        if (currentWeightText != null)
        {
            currentWeightText.text = currentWeight.ToString();
        }
    }

    // Método para obtener el peso actual (útil para UI)
    public int GetCurrentWeight()
    {
        return currentWeight;
    }

    // Método para verificar si el puzzle está resuelto
    public bool IsSolved()
    {
        return puzzleSolved;
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = puzzleSolved ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>()?.size ?? Vector3.one);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UpdateDisplay();
        }
    }
}
