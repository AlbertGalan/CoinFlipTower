using System;

[Serializable]
public class RateGameDTO
{
    public string api_token { get; set; }
    public string email { get; set; }
    public string name { get; set; }
    public int general { get; set; }
    public int jugabilitat { get; set; }
    public int dificultat { get; set; }
    public int grafics { get; set; }
    public int concordancia { get; set; }
}
