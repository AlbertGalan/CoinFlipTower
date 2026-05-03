using UnityEngine;

public class LavaBreathing : MonoBehaviour
{
    public Transform floorLava;
    public Transform roofLava;

    [Header("Ajustes Suelo")]
    public float floorMinY = 0f;
    public float floorMaxY = 5f;

    [Header("Ajustes Techo")]
    public float roofMaxY = 20f; // Lava bajada
    public float roofMinY = 25f; // Lava subida

    public float speed = 1f;

    void Update()
    {
        // Calculamos un valor de 0 a 1 que oscila
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        // Suelo sube mientras techo baja
        floorLava.position = new Vector3(floorLava.position.x, Mathf.Lerp(floorMinY, floorMaxY, t), floorLava.position.z);
        roofLava.position = new Vector3(roofLava.position.x, Mathf.Lerp(roofMinY, roofMaxY, t), roofLava.position.z);
    }
}