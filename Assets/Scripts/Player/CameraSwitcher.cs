using UnityEngine;

/// <summary>
/// Simple camera switcher for prototype: toggles two Camera GameObjects.
/// Assign the existing main (first-person) Camera and a top-down Camera in the Inspector.
/// </summary>
public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher Instance { get; private set; }

    [Tooltip("Main first-person Camera GameObject (usually the active Main Camera)")]
    public GameObject firstPersonCameraGO;
    [Tooltip("Top-down Camera GameObject (disabled by default)")]
    public GameObject topDownCameraGO;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ensure sensible default: FP active if assigned
        if (firstPersonCameraGO != null)
            firstPersonCameraGO.SetActive(true);
        if (topDownCameraGO != null)
            topDownCameraGO.SetActive(false);
    }

    public void SetFirstPerson()
    {
        if (firstPersonCameraGO != null) firstPersonCameraGO.SetActive(true);
        if (topDownCameraGO != null) topDownCameraGO.SetActive(false);
    }

    public void SetTopDown()
    {
        if (firstPersonCameraGO != null) firstPersonCameraGO.SetActive(false);
        if (topDownCameraGO != null) topDownCameraGO.SetActive(true);
    }
}
