using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Score : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    //Variable puntuació
    float score;
    //Timer en minuts, segons i mil·lesimes de segon, Formatar!!
    float timer;


    float pointsPerSecond = 1f;
    float gravityChangePenalty = 5f;

    //Referenciam scripts de gravetat per poder detectar quan un objecte o quan el jugador canvia de gravetat i restar-li puntuació en funció.
    private Gravetat playerGravity;
    private InteractGravity interactGravity;
    void Start()
    {
        playerGravity = GetComponent<Gravetat>();
        interactGravity = GetComponent<InteractGravity>();

        score = 1000f;
        timer = 0f;

    }

    // Update is called once per frame
    void Update()
    {

        timer += Time.deltaTime;

        score -= pointsPerSecond * Time.deltaTime;

        Debug.Log("Temps:" + " " + timer + " " + "Puntuació:" + score);


    }
    

}
