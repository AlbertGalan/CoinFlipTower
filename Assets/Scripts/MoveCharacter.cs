using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class MoveCharacter : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {

        //Manera de moure dreta esquerre 
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");
        Vector3 patata = new Vector3(xDirection, 0.0f, zDirection);


        transform.position += patata * 0.1f;
    
    }
}
