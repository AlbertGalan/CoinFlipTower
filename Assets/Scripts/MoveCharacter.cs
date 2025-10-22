using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class MoveCharacter : MonoBehaviour
{

    // Update is called once per frame
    public Transform personatge;

    public float speed = 5f;

    bool gravetat = false;
    void Update()
    {

        UnityEngine.Vector3 direccioPersonatge = UnityEngine.Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            direccioPersonatge += UnityEngine.Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            direccioPersonatge += UnityEngine.Vector3.back;
        }

        if (Input.GetKey(KeyCode.A))
        {
            direccioPersonatge += UnityEngine.Vector3.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            direccioPersonatge += UnityEngine.Vector3.right;
        }
        transform.Translate(direccioPersonatge * speed * Time.deltaTime);

           //Manera de moure dreta esquerre 
            //float xDirection = Input.GetAxis("Horizontal");
            //float zDirection = Input.GetAxis("Vertical");
            //Vector3 patata = new Vector3(xDirection, 0.0f, zDirection);


            //transform.position += patata * 0.1f;


    }
}