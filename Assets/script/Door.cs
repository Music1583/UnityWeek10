using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class Door : MonoBehaviour
{
    [SerializeField] private Key requiredKey;
    [SerializeField] private KeyColor keyColor = KeyColor.Blue;
    [SerializeField] [Min(1)] private int keysNeeded = 1;

    [Header("Door Renderers")]
    [SerializeField] private SpriteRenderer bottomRenderer;
    [SerializeField] private SpriteRenderer topRenderer;

    [Header("Door Sprites")]
    [SerializeField] private Sprite doorClosedSprite;
    [SerializeField] private Sprite doorClosedTopSprite;
    [SerializeField] private Sprite doorOpenSprite;
    [SerializeField] private Sprite doorOpenTopSprite;

    [Header("Indicator")]
    [SerializeField] private GameObject indicatorRoot;
    [SerializeField] private SpriteRenderer indicatorKeyRenderer;
    [SerializeField] private TMP_Text indicatorCountText;

    [Header("HUD Key Sprites")]
    [SerializeField] private Sprite hudKeyBlue;
    [SerializeField] private Sprite hudKeyGreen;
    [SerializeField] private Sprite hudKeyRed;
    [SerializeField] private Sprite hudKeyYellow;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;

    private BoxCollider2D doorCollider;
    private bool isOpen = false;

    public KeyColor RequiredColor => keyColor;
    public int KeysNeeded => keysNeeded;
    public bool IsOpen => isOpen;

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        SyncFromRequiredKey();
        UpdateVisuals();
    }

    private void OnValidate()
    {
        if (doorCollider == null) doorCollider = GetComponent<BoxCollider2D>();
        SyncFromRequiredKey();
        UpdateVisuals();
    }

    private void SyncFromRequiredKey()
    {
        if (requiredKey != null)
        {
            keyColor = requiredKey.Color;
        }
    }

    public void UpdateVisuals()
    {
        if (bottomRenderer != null && doorClosedSprite != null && !isOpen)
            bottomRenderer.sprite = doorClosedSprite;
        if (topRenderer != null && doorClosedTopSprite != null && !isOpen)
            topRenderer.sprite = doorClosedTopSprite;

        if (indicatorKeyRenderer != null)
        {
            Sprite icon = keyColor switch
            {
                KeyColor.Blue => hudKeyBlue,
                KeyColor.Green => hudKeyGreen,
                KeyColor.Red => hudKeyRed,
                KeyColor.Yellow => hudKeyYellow,
                _ => hudKeyBlue
            };
            indicatorKeyRenderer.sprite = icon;
        }

        if (indicatorCountText != null)
        {
            indicatorCountText.text = $"x{keysNeeded}";
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isOpen) return;

        PayerController player = collision.gameObject.GetComponent<PayerController>();
        if (player == null) player = collision.gameObject.GetComponentInParent<PayerController>();

        if (player != null)
        {
            TryOpen(player);
        }
    }

    public bool TryOpen(PayerController player)
    {
        if (isOpen) return true;

        int held = player.GetKeyCount(keyColor);
        if (held >= keysNeeded)
        {
            player.UseKeys(keyColor, keysNeeded);
            OpenDoor();
            return true;
        }
        else
        {
            int missing = keysNeeded - held;
            string msg = $"Need {missing} more {keyColor} key{(missing > 1 ? "s" : "")}!";
            Debug.Log(msg);
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowFeedbackMessage(msg);
            }
            return false;
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        if (bottomRenderer != null && doorOpenSprite != null)
            bottomRenderer.sprite = doorOpenSprite;
        if (topRenderer != null && doorOpenTopSprite != null)
            topRenderer.sprite = doorOpenTopSprite;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (indicatorRoot != null)
            indicatorRoot.SetActive(false);

        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }
}

