using TMPro;
using UnityEngine;

public class ClassificationRowUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

   public void SetData(int position, string name, int score)
{
    if (positionText != null)
        positionText.text = position.ToString();
    else
        Debug.LogError("positionText es NULL en " + gameObject.name);
    
    if (nameText != null)
        nameText.text = name;
    else
        Debug.LogError("nameText es NULL en " + gameObject.name);
    
    if (scoreText != null)
        scoreText.text = score.ToString();
    else
        Debug.LogError("scoreText es NULL en " + gameObject.name);
}
}
