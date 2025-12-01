using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> PROVISIONAL
/// Simple visual highlight for grabbable objects.
/// Adds an emission tint while highlighted and supports a pulsing effect.
/// Attach to any object you want to show a visual when the player is near.
/// </summary>
public class GrabbableVisual : MonoBehaviour
{
    [Tooltip("Color used when highlighting the object")]
    public Color highlightColor = Color.yellow;

    [Tooltip("Pulse speed of emission (0 = steady)")]
    public float pulseSpeed = 2f;

    [Tooltip("Maximum emission intensity multiplier")]
    public float emissionIntensity = 1.2f;

    private Renderer[] rends;
    private Color[] originalEmissionColors;
    private bool[] hadEmissionKeyword;
    private Coroutine pulseCoroutine;

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();
        originalEmissionColors = new Color[rends.Length];
        hadEmissionKeyword = new bool[rends.Length];
        for (int i = 0; i < rends.Length; i++)
        {
            var mat = rends[i].material;
            bool had = mat.IsKeywordEnabled("_EMISSION");
            hadEmissionKeyword[i] = had;
            if (had)
            {
                if (mat.HasProperty("_EmissionColor"))
                    originalEmissionColors[i] = mat.GetColor("_EmissionColor");
                else
                    originalEmissionColors[i] = Color.black;
            }
            else
            {
                originalEmissionColors[i] = Color.black;
            }
        }
    }

    public void Highlight(bool on)
    {
        if (on)
        {
            if (pulseSpeed > 0f)
            {
                if (pulseCoroutine == null) pulseCoroutine = StartCoroutine(PulseCoroutine());
            }
            else
            {
                ApplyEmission(highlightColor * emissionIntensity);
            }
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            RestoreOriginalEmission();
        }
    }

    private IEnumerator PulseCoroutine()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float f = (Mathf.Sin(t) * 0.5f) + 0.5f; // 0..1
            Color c = highlightColor * (0.5f + f * 0.5f) * emissionIntensity;
            ApplyEmission(c);
            yield return null;
        }
    }

    private void ApplyEmission(Color c)
    {
        for (int i = 0; i < rends.Length; i++)
        {
            var mat = rends[i].material;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", c);
            }
        }
    }

    private void RestoreOriginalEmission()
    {
        for (int i = 0; i < rends.Length; i++)
        {
            var mat = rends[i].material;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", originalEmissionColors[i]);
                if (!hadEmissionKeyword[i]) mat.DisableKeyword("_EMISSION");
            }
        }
    }

    private void OnDestroy()
    {
        RestoreOriginalEmission();
    }
}
