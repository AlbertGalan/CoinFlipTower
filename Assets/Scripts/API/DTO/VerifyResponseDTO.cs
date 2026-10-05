using System;
using System.Collections.Generic;

[Serializable]
public class VerifyResponseDTO
{
    public bool rated { get; set; }
    public List<CriterionDTO> criterion { get; set; }
}
