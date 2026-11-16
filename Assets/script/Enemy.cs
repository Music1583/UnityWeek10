using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float leftDistance = 2f;
    [SerializeField] private float rightDistance = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 startPosition;
    private float leftLimitX;
    private float rightLimitX;
    private bool movingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        leftLimitX = startPosition.x - leftDistance;
        rightLimitX = startPosition.x + rightDistance;
    }

    private void Start()
    {
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        float currentX = rb.position.x;
        float dir = movingRight ? 1f : -1f;
        float nextX = currentX + dir * moveSpeed * Time.fixedDeltaTime;

        if (movingRight && nextX >= rightLimitX)
        {
            nextX = rightLimitX;
            movingRight = false;
            UpdateFacing();
        }
        else if (!movingRight && nextX <= leftLimitX)
        {
            nextX = leftLimitX;
            movingRight = true;
            UpdateFacing();
        }

        rb.MovePosition(new Vector2(nextX, rb.position.y));
    }

    private void UpdateFacing()
    {
        if (spriteRenderer != null)
        {
            // Kenney enemy sprite (slime_spike) faces left by default.
            // When moving right, flipX = true; when moving left, flipX = false.
            spriteRenderer.flipX = movingRight;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PayerController player = other.GetComponent<PayerController>();
        if (player == null)
        {
            player = other.GetComponentInParent<PayerController>();
        }

        if (player != null)
        {
            player.TakeDamage(1);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = Application.isPlaying ? (Vector3)startPosition : transform.position;
        Vector3 leftPoint = new Vector3(origin.x - leftDistance, origin.y, origin.z);
        Vector3 rightPoint = new Vector3(origin.x + rightDistance, origin.y, origin.z);

        // Line between the two limits at the enemy's height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(leftPoint, rightPoint);

        // Red wire sphere at the left limit
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftPoint, 0.25f);

        // Green wire sphere at the right limit
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(rightPoint, 0.25f);
    }
}
