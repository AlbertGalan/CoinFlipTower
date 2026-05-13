using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;

    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioSource audioSource;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
public void Activate()
{
    Debug.Log("ACTIVAT");
    animator.SetBool("isOpen", true);
    if (openSound != null && audioSource != null)
    {
        audioSource.PlayOneShot(openSound);
    }
}

public void Deactivate()
{
    Debug.Log("DESACTIVAT");
    if (closeSound != null && audioSource != null)
    {
        audioSource.PlayOneShot(closeSound);
    }
    animator.SetBool("isOpen", false);
}

}
