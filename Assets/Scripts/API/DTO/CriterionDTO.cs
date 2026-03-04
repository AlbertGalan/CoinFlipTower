using System;

[Serializable]
public class CriterionDTO
{
    public string name { get; set; }
    public int min_score { get; set; }
    public int max_score { get; set; }
}
