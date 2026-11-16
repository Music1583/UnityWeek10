using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Goal : MonoBehaviour
{
    [Header("Door Requirement")]
    [Tooltip("The door that must be opened before reaching the goal. If left empty, will automatically find the Door in the scene.")]
    [SerializeField] private Door requiredDoor;

    [Header("Audio")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip warpSound;

    private bool reached = false;

    private void Start()
    {
        if (requiredDoor == null)
        {
            requiredDoor = FindFirstObjectByType<Door>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reached) return;

        PayerController player = other.GetComponent<PayerController>()
                              ?? other.GetComponentInParent<PayerController>();

        if (player == null) return;

        // Check if the door has been opened
        if (!IsDoorOpened())
        {
            // If the door is not open: warp player back to spawn/respawn point
            player.Respawn();

            if (warpSound != null)
            {
                AudioSource.PlayClipAtPoint(warpSound, transform.position);
            }

            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowFeedbackMessage("You must open the door first!");
            }
            return;
        }

        // Door is open: trigger victory celebration sequence
        StartCoroutine(WinSequence(player));
    }

    private IEnumerator WinSequence(PayerController player)
    {
        reached = true;

        if (winSound != null)
        {
            AudioSource.PlayClipAtPoint(winSound, transform.position);
        }

        // Stop walking and jump in celebration!
        player.Celebrate();

        // Wait for celebratory jump to peak and land before showing the UI
        yield return new WaitForSeconds(1.3f);

        if (GameUI.Instance != null)
        {
            GameUI.Instance.ShowEndScreen("Level Complete!", "Restart");
        }
    }

    private bool IsDoorOpened()
    {
        if (requiredDoor != null)
        {
            return requiredDoor.IsOpen;
        }

        Door[] doors = FindObjectsByType<Door>(FindObjectsSortMode.None);
        foreach (var door in doors)
        {
            if (!door.IsOpen) return false;
        }
        return true;
    }
}