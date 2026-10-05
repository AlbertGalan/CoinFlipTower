using UnityEngine;

public class CollisionDebugger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name != "Enterra")
        Debug.Log("COLISIONO con: " + collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name != "Enterra")
        Debug.Log("TRIGGER con: " + other.gameObject.name);
    }
}
