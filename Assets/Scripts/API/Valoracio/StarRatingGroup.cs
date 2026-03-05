using UnityEngine;

public class StarRatingGroup : MonoBehaviour
{
    public StarRating[] stars;
    public int currentRating = 0;

    public void SetRating(int rating)
    {
        currentRating = rating;

        for (int i = 0; i < stars.Length; i++)
        {
            if (i < rating)
                stars[i].SetFilled(true);
            else
                stars[i].SetFilled(false);
        }
    }
}