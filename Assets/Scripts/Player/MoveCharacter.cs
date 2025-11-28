using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveCharacter : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 90f;

    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        float inputH = Input.GetAxis("Horizontal");
        float inputV = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.forward * inputV + transform.right * inputH).normalized;

        Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    void Update()
    {
        // --- Rotació ratolí ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        //rotar el personatge
        transform.Rotate(Vector3.up * mouseX);

        // Rotar camara verticalment amb limitacions
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
