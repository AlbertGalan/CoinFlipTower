using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
public void Activate()
{
    Debug.Log("PUERTA ACTIVADA");
    animator.SetBool("isOpen", true);
}

public void Deactivate()
{
    Debug.Log("PUERTA DESACTIVADA");
    animator.SetBool("isOpen", false);
}

}
