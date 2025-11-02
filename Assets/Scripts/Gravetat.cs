using UnityEngine;

public class Gravetat : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame

    public float gravityForce = 9.81f; //Intensitat gravetat
    private Rigidbody rb;
    
    private bool invertit = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Pitjada tecla g");
            invertit = !invertit;

            transform.Rotate(180f, 0f, 0f);
        }
    }
     void FixedUpdate()
    {
        // Aplicar fuerza de gravedad personalizada
        Vector3 gravityDirection = invertit ? Vector3.up : Vector3.down;
        rb.AddForce(gravityDirection * gravityForce, ForceMode.Acceleration);
    }
}
