using UnityEngine;

public class StarRating : MonoBehaviour
{
    public int starIndex;
    public StarRatingGroup group;

    public Renderer starRenderer;

    public Material emptyMaterial;
    public Material filledMaterial;

    void OnMouseDown()
    {
        group.SetRating(starIndex);
    }

    public void SetFilled(bool filled)
    {
        if (filled)
            starRenderer.material = filledMaterial;
        else
            starRenderer.material = emptyMaterial;
    }
}