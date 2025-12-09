using UnityEngine;

/// <summary>
/// Small helper component you can place on trigger colliders to show a given
/// tutorial step when the player enters/exits the trigger.
/// Requires a Collider with `isTrigger = true`.
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    public enum TriggerMode { OnEnter, OnExit }

    [Tooltip("Index of the tutorial panel (0-based) to show when triggered")]
    public int stepIndex = 0;

    public enum SelectMode { ByIndex, ByPanel }
    [Tooltip("How to select which panel to show: by index (uses stepIndex) or by direct Panel GameObject reference")]
    public SelectMode selectMode = SelectMode.ByIndex;
    [Tooltip("Optional direct reference to a panel GameObject. Used when SelectMode = ByPanel")]
    public GameObject panelReference;

    [Tooltip("When to trigger the step")]
    public TriggerMode mode = TriggerMode.OnEnter;

    [Tooltip("If true this trigger will only fire once")]
    public bool oneShot = true;

    [Tooltip("Optional tag filter for the collider (e.g. Player). Leave empty to accept any Collider.)")]
    public string requiredTag = "Player";

    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (mode != TriggerMode.OnEnter) return;
        if (oneShot && fired) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
        if (TutorialMessageManager.Instance != null)
        {
            FireTriggerAction();
            fired = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (mode != TriggerMode.OnExit) return;
        if (oneShot && fired) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
        if (TutorialMessageManager.Instance != null)
        {
            FireTriggerAction();
            fired = true;
        }
    }

    private void FireTriggerAction()
    {
        if (selectMode == SelectMode.ByPanel && panelReference != null)
        {
            // Try to show by panel reference first
            if (!TutorialMessageManager.Instance.ShowPanel(panelReference))
            {
                // fallback to index
                TutorialMessageManager.Instance.ShowStep(stepIndex);
            }
        }
        else
        {
            TutorialMessageManager.Instance.ShowStep(stepIndex);
        }
    }
}
