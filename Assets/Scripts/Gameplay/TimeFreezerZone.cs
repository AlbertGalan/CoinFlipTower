using UnityEngine;

public class TimeFreezerZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Score.Instance != null)
            {
                Score.Instance.FreezeGameplay();
                Debug.Log("<color=yellow>Tiempo detenido:</color> Estás en la zona segura.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Score.Instance != null)
            {
                Score.Instance.UnfreezeGameplay();
                Debug.Log("<color=green>Tiempo reanudado:</color> Has salido de la zona segura.");
            }
        }
    }
}