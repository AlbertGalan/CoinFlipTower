using UnityEngine;

public class SpawnMeteor : MonoBehaviour
{
    public GameObject meteorPrefab;

    public GameObject cubePoint;

    private GameObject meteor;
    public float velocitat = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 meteorSpawnPoint = cubePoint.transform.position;

            //per poder moure un objecte spawneat hem de guardar on s'ha spawneat
            meteor = Instantiate(meteorPrefab, meteorSpawnPoint, Quaternion.identity);
            Debug.Log("Pitjada tecla E");
        }
        
            if (meteor != null)
            {
            Vector3 direccio = new Vector3(0, -1, 0);
                meteor.transform.Translate(direccio * velocitat * Time.deltaTime);
            
            }
    }
}