using UnityEngine;

public class MovingMirrorPlatforms : MonoBehaviour
{
    public float distance = 10f;
    public float speed = 2f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Movimiento Ping-Pong en el eje Z
        float offset = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPos + new Vector3(0, 0, offset);
    }
}