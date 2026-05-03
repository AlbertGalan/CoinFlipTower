using UnityEngine;
using UnityEngine.Events;

public class SimpleDoubleButtonManager : MonoBehaviour
{
    public int requiredButtons = 2;
    public GameObject portalObject; // El portal que aparecerá
    public UnityEvent onAllButtonsActivated; // Por si quieres añadir sonidos o efectos extra

    private int currentCount = 0;

    void Start()
    {
        if (portalObject != null) portalObject.SetActive(false);
    }

    public void NotifyButtonActivated()
    {
        currentCount++;

        if (currentCount >= requiredButtons)
        {
            ShowPortal();
        }
    }

    private void ShowPortal()
    {
        if (portalObject != null) portalObject.SetActive(true);
        onAllButtonsActivated.Invoke();
        Debug.Log("¡Portal activado!");
    }
}