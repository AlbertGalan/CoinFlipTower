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
        
        if (nameText != null)
            nameText.text = name;
        
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}
