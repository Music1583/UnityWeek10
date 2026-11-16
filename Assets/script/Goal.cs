using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Goal : MonoBehaviour
{
    [SerializeField] private AudioClip winSound;
    private bool reached = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reached) return;

        PayerController player = other.GetComponent<PayerController>();
        if (player == null) player = other.GetComponentInParent<PayerController>();

        if (player != null)
        {
            reached = true;
            player.SetCanControl(false);

            if (winSound != null)
            {
                AudioSource.PlayClipAtPoint(winSound, transform.position);
            }

            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowEndScreen("Level Complete!", "Restart");
            }
        }
    }
}

