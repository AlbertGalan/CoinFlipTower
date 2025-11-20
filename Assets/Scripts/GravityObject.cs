using Unity.VisualScripting;
using UnityEngine;

public class GravityObject : MonoBehaviour
{
    public float gravityForce = 9.81f;
    public bool gravityInverted = false;
    
    private Rigidbody rb;
    private Vector3 currentGravityDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        UpdateGravityDirection();
    }

    void FixedUpdate()
    {
        rb.AddForce(currentGravityDirection * gravityForce, ForceMode.Acceleration);
    }

    public void ToggleGravity()
    {
        gravityInverted = !gravityInverted;
        UpdateGravityDirection();
    }
 
    public void SetGravityInverted(bool inverted)
    {
        gravityInverted = inverted;
        UpdateGravityDirection();
    }

    public void UpdateGravityDirection()
    {
        currentGravityDirection = gravityInverted ? Vector3.up : Vector3.down;
    }
    public bool IsGravityInverted()
    {
        return gravityInverted;
    }
}