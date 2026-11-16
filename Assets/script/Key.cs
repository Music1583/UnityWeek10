using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Key : MonoBehaviour
{
    [SerializeField] private KeyColor keyColor = KeyColor.Blue;
    [SerializeField] private AudioClip pickupSound;

    [Header("Sprites")]
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite yellowSprite;

    private SpriteRenderer spriteRenderer;

    public KeyColor Color => keyColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public void SetColor(KeyColor newColor)
    {
        keyColor = newColor;
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        Sprite selected = keyColor switch
        {
            KeyColor.Blue => blueSprite,
            KeyColor.Green => greenSprite,
            KeyColor.Red => redSprite,
            KeyColor.Yellow => yellowSprite,
            _ => blueSprite
        };

        if (selected != null)
        {
            spriteRenderer.sprite = selected;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Application.isPlaying) return;

        PayerController player = other.GetComponent<PayerController>();
        if (player == null) player = other.GetComponentInParent<PayerController>();

        if (player != null)
        {
            player.AddKey(keyColor);
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            Destroy(gameObject);
        }
    }
}
