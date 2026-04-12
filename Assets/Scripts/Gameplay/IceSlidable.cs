using UnityEngine;

public class IceSlidable : MonoBehaviour 
{
    private Rigidbody rb;
    private bool sliding = false;
    private Vector3 dir;
    public float slideSpeed = 12f;

    void Start() => rb = GetComponent<Rigidbody>();

    public void StartSliding(Vector3 direction) 
    {
        sliding = true;
        dir = direction;
    }

    void FixedUpdate() 
    {
        if (!sliding) return;

        // Si choca o sale del trigger de hielo, se para
        rb.linearVelocity = dir * slideSpeed;
    }

    private void OnCollisionEnter(Collision collision) => sliding = false;
    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("IceZone")) sliding = false;
    }
}