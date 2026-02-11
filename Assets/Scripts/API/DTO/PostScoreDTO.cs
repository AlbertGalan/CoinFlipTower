using System;
using Newtonsoft.Json;

[Serializable]
public class PostScoreDTO
{
    public string name { get; set; }
    public string email { get; set; }
    public int puntuacion { get; set; }

    public string api_token { get; set; }

}
