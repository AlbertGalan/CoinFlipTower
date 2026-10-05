using UnityEngine;
using System.Collections;

public class LavaHazard : MonoBehaviour
{
    [Header("Efectos")]
    public float fadeDuration = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RespawnSequence(other.gameObject));
        }
    }

    IEnumerator RespawnSequence(GameObject player)
    {
        var moveScript = player.GetComponent<MoveCharacter>();
        if (moveScript != null) moveScript.SetBlockMovement(true);

        player.SendMessage("Release", SendMessageOptions.DontRequireReceiver);

        yield return new WaitForSeconds(0.2f); 

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer(player);
        }

        yield return new WaitForSeconds(fadeDuration);

        if (moveScript != null) moveScript.SetBlockMovement(false);
    }
}