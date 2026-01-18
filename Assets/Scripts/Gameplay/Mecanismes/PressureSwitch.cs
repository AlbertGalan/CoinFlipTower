using UnityEngine;
using UnityEngine.Events;

public class PressureSwitch : MonoBehaviour
{
    private Animator animator;

    public UnityEvent OnPressed;
    public UnityEvent OnReleased;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pushable"))
        {
            animator.SetBool("isPressed", true);
            OnPressed.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pushable"))
        {
            animator.SetBool("isPressed", false);
            OnReleased.Invoke();
        }
    }
}