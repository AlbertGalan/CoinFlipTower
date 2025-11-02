using UnityEngine;

public class InteractGravity : MonoBehaviour
{
    public float interactionRange = 3f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Array de objectes d'aprop
            Collider[] objectsNearby = Physics.OverlapSphere(transform.position, interactionRange);
            
            foreach (Collider col in objectsNearby)
            {
                GravityObject gravityObj = col.GetComponent<GravityObject>();
                if (gravityObj != null)
                {
                    gravityObj.ToggleGravity();
                }
            }
        }
    }
}