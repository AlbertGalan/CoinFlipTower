using TMPro;
using UnityEngine;

public class FadeAwayText : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float fadeTime;
    [SerializeField]
    private TextMeshProUGUI fadeAwayText;
    private float alphaValue;
    private float fadeAwayPerSecond;
    void Start()
    {
        fadeAwayText = GetComponent<TextMeshProUGUI>();
        fadeAwayPerSecond = 1f / fadeTime;
        alphaValue = fadeAwayText.color.a;

    }

    // Update is called once per frame
    void Update()
    {
        if( fadeTime > 0f)
        {
            alphaValue -= fadeAwayPerSecond * Time.deltaTime;
            fadeAwayText.color = new Color(fadeAwayText.color.r, fadeAwayText.color.g, fadeAwayText.color.b, alphaValue);
            fadeTime -= Time.deltaTime;
        }
    }
}
