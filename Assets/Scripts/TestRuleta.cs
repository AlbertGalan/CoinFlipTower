using UnityEngine;

public class TestRuleta : MonoBehaviour 
{
    public RouletteWheel miRuleta;

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            miRuleta.Spin();
        }
    }
}