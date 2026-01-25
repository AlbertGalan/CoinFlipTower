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
    Debug.Log("ACTIVAT");
    animator.SetBool("isOpen", true);
}

public void Deactivate()
{
    Debug.Log("DESACTIVAT");
    animator.SetBool("isOpen", false);
}

}
