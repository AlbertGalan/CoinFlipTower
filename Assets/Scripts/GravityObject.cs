using UnityEngine;

public class GravityObject : MonoBehaviour
{
    public float gravityForce = 9.81f;
    public bool gravityInverted = false;
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        Vector3 gravityDirection = gravityInverted ? Vector3.up : Vector3.down;
        rb.AddForce(gravityDirection * gravityForce, ForceMode.Acceleration);
    }

    public void SetGravityInverted(bool inverted)
    {
        gravityInverted = inverted;
    }

    public void ToggleGravity()
    {
        gravityInverted = !gravityInverted;
    }
}