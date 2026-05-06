using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialToLevelTrigger : MonoBehaviour
{
    public string nextSceneName = "PisNivell";
    public string playerTag = "Player";
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag(playerTag)) return;
        hasTriggered = true;

        // Guardem les dades actuals en claus temporals per al següent nivell
        if (Score.Instance != null)
        {
            PlayerPrefs.SetFloat("SavedScore", Score.Instance.score);
            PlayerPrefs.SetFloat("SavedTime", Score.Instance.timer);
            PlayerPrefs.Save();
            Debug.Log("Dades del Tutorial desades a PlayerPrefs per al següent nivell.");
        }

        SceneManager.LoadScene(nextSceneName);
    }
}