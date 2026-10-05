using UnityEngine;
using UnityEngine.Events;

public class PressureSwitch : MonoBehaviour
{
    private Animator animator;

    public UnityEvent OnPressed;
    public UnityEvent OnReleased;
    public Animator placa;

    void Start()
    {
       // animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pushable"))
        {
            placa.SetBool("isPressed", true);
            OnPressed.Invoke();
            Debug.Log("Switch presionat");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pushable"))
        {
            placa.SetBool("isPressed", false);
            OnReleased.Invoke();
            Debug.Log("Switch alliberat");
        }
    }
}