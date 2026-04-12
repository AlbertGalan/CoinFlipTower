using System.Collections.Generic;
using UnityEngine;

public sealed class GameEnviroment
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private static GameEnviroment instance;
    private List<GameObject>  checkpoints = new List<GameObject>();
    
    public List<GameObject> Checkpoints { get{ return checkpoints;}}

    public static GameEnviroment Singleton
    {
        get
        {
            if (instance == null)
            {
                instance = new GameEnviroment();
                instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
            }
            return instance;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
