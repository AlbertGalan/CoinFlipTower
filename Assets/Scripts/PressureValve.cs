using UnityEngine;

public class PressureValve : MonoBehaviour
{
    [Header("Referencias")]
    public PressurePuzzleManager manager;
    public GravityController valveObjectGravity; // Arrastra aquí el objeto que sube/baja

    [Header("Configuración")]
    public int valueContribution = 5; // Lo que aporta esta válvula
    
    private bool isActive = false;

    void Update()
    {
        if (valveObjectGravity == null || manager == null) return;

        // Comprobamos el estado de la gravedad del objeto
        // Si la gravedad está invertida (está en el techo), la válvula se considera "Abierta"
        bool shouldBeActive = valveObjectGravity.IsGravityInverted();

        // Solo notificamos al manager si el estado ha cambiado para evitar bucles
        if (shouldBeActive != isActive)
        {
            isActive = shouldBeActive;
            
            if (isActive)
            {
                manager.AddPressure(valueContribution);
                Debug.Log($"<color=blue>Válvula Activada:</color> +{valueContribution} de presión.");
            }
            else
            {
                manager.AddPressure(-valueContribution);
                Debug.Log($"<color=orange>Válvula Desactivada:</color> -{valueContribution} de presión.");
            }
            
            // Aquí podrías añadir una animación de la válvula girando
        }
    }
}