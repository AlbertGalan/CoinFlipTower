using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a sequence of tutorial message panels (GameObjects).
/// Drag your 7 message panels (UI GameObjects inside Canvas) into `messagePanels`.
/// The manager shows one at a time and optionally fades between them.
/// Call `Next()`, `Prev()` or `ShowStep(index)` from other scripts or TutorialTrigger components.
/// </summary>
public class TutorialMessageManager : MonoBehaviour
{
    public static TutorialMessageManager Instance { get; private set; }

    [Tooltip("List of message panel GameObjects (order = tutorial order). Only one will be active at a time.")]
    public GameObject[] messagePanels;

    [Tooltip("Fade duration in seconds when switching panels (0 = instant)")]
    public float fadeDuration = 0.25f;

    [Tooltip("If true the manager will auto-advance after `autoAdvanceDelay` for panels that have AutoAdvance=true")]
    public bool autoAdvance = false;
    [Tooltip("Default delay for auto-advancing (seconds)")]
    public float autoAdvanceDelay = 3f;
    [Header("Startup")]
    [Tooltip("If true the manager will show the Start Index panel when the scene starts")]
    public bool showOnStart = true;
    [Tooltip("Index of the panel to show on start (0-based)")]
    public int startIndex = 0;

    private int currentIndex = -1;
    private CanvasGroup[] canvasGroups;
    private Coroutine transitionCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Prepare CanvasGroups for panels (add if missing)
        if (messagePanels != null && messagePanels.Length > 0)
        {
            canvasGroups = new CanvasGroup[messagePanels.Length];
            for (int i = 0; i < messagePanels.Length; i++)
            {
                var go = messagePanels[i];
                if (go == null) continue;
                var cg = go.GetComponent<CanvasGroup>();
                if (cg == null) cg = go.AddComponent<CanvasGroup>();
                canvasGroups[i] = cg;
                // start all panels hidden
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                go.SetActive(false);
            }
        }

        // Start at first step if available
        if (showOnStart && messagePanels != null && messagePanels.Length > 0)
        {
            int idx = Mathf.Clamp(startIndex, 0, messagePanels.Length - 1);
            ShowStep(idx, instant:true);
        }
    }

    /// <summary>
    /// Show the panel associated with the given GameObject (if it exists in the manager list).
    /// Returns true if the panel was found and shown.
    /// </summary>
    public bool ShowPanel(GameObject panel, bool instant = false)
    {
        if (panel == null || messagePanels == null) return false;
        for (int i = 0; i < messagePanels.Length; i++)
        {
            if (messagePanels[i] == panel)
            {
                ShowStep(i, instant);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Helper to get the panel display names (for editor dropdowns).
    /// </summary>
    public string[] GetPanelNames()
    {
        if (messagePanels == null) return new string[0];
        string[] names = new string[messagePanels.Length];
        for (int i = 0; i < messagePanels.Length; i++)
            names[i] = (messagePanels[i] != null) ? messagePanels[i].name : "(Empty)";
        return names;
    }

    /// <summary>Show panel at index (0-based). If instant=true skip fading.</summary>
    public void ShowStep(int index, bool instant = false)
    {
        if (messagePanels == null || index < 0 || index >= messagePanels.Length) return;
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionTo(index, instant));
    }

    public void Next()
    {
        if (messagePanels == null || messagePanels.Length == 0) return;
        int next = Mathf.Clamp(currentIndex + 1, 0, messagePanels.Length - 1);
        ShowStep(next);
    }

    public void Prev()
    {
        if (messagePanels == null || messagePanels.Length == 0) return;
        int prev = Mathf.Clamp(currentIndex - 1, 0, messagePanels.Length - 1);
        ShowStep(prev);
    }

    public int CurrentIndex => currentIndex;

    private IEnumerator TransitionTo(int newIndex, bool instant)
    {
        // If same index, do nothing
        if (newIndex == currentIndex) yield break;

        // hide old
        if (currentIndex >= 0 && currentIndex < messagePanels.Length)
        {
            var oldGO = messagePanels[currentIndex];
            var oldCg = canvasGroups[currentIndex];
            if (oldGO != null && oldCg != null)
            {
                if (instant || fadeDuration <= 0f)
                {
                    oldCg.alpha = 0f;
                    oldCg.interactable = false;
                    oldCg.blocksRaycasts = false;
                    oldGO.SetActive(false);
                }
                else
                {
                    float t = 0f;
                    float from = oldCg.alpha;
                    while (t < fadeDuration)
                    {
                        t += Time.deltaTime;
                        oldCg.alpha = Mathf.Lerp(from, 0f, t / fadeDuration);
                        yield return null;
                    }
                    oldCg.alpha = 0f;
                    oldCg.interactable = false;
                    oldCg.blocksRaycasts = false;
                    oldGO.SetActive(false);
                }
            }
        }

        // show new
        var newGO = messagePanels[newIndex];
        var newCg = canvasGroups[newIndex];
        if (newGO != null && newCg != null)
        {
            newGO.SetActive(true);
            if (instant || fadeDuration <= 0f)
            {
                newCg.alpha = 1f;
                newCg.interactable = true;
                newCg.blocksRaycasts = true;
            }
            else
            {
                float t = 0f;
                newCg.alpha = 0f;
                newCg.interactable = false;
                newCg.blocksRaycasts = false;
                while (t < fadeDuration)
                {
                    t += Time.deltaTime;
                    newCg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                    yield return null;
                }
                newCg.alpha = 1f;
                newCg.interactable = true;
                newCg.blocksRaycasts = true;
            }
        }

        currentIndex = newIndex;

        // Auto-advance if enabled (for simple cases). Panels can be advanced automatically.
        if (autoAdvance)
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            int nxt = Mathf.Min(currentIndex + 1, messagePanels.Length - 1);
            if (nxt != currentIndex) ShowStep(nxt);
        }
    }
}
