using System;
using UnityEngine;

public class StarRatingGroup : MonoBehaviour
{
    public StarRating[] stars;
    public int currentRating = 0;
    public event Action<int> OnRatingChanged;

    public void SetRating(int rating)
    {
        int maxRating = stars != null ? Mathf.Min(stars.Length, 5) : 5;
        currentRating = Mathf.Clamp(rating, 0, maxRating);

        if (stars == null)
        {
            OnRatingChanged?.Invoke(currentRating);
            return;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            if (i < currentRating)
                stars[i].SetFilled(true);
            else
                stars[i].SetFilled(false);
        }

        OnRatingChanged?.Invoke(currentRating);
    }
}