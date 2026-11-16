using UnityEngine;

/// <summary>
/// Patrol enemy supporting Horizontal (left-right) and Vertical (up-down) modes.
/// Supports stomping mechanics: player bounces on head, enemy plays death animation and sound.
/// Uses Rigidbody2D.MovePosition in FixedUpdate (unity-physics) and Animator triggers (unity-animation).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    public enum PatrolAxis { Horizontal, Vertical }

    [Header("Patrol")]
    [Tooltip("Horizontal = left-right, Vertical = up-down")]
    [SerializeField] private PatrolAxis patrolAxis = PatrolAxis.Horizontal;
    [Tooltip("Speed while patrolling (units / sec)")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("Distance toward negative direction from start (left / down)")]
    [SerializeField] private float leftDistance = 2f;
    [Tooltip("Distance toward positive direction from start (right / up)")]
    [SerializeField] private float rightDistance = 2f;

    [Header("Stomp & Death")]
    [Tooltip("Can the player stomp on this enemy to kill it?")]
    [SerializeField] private bool canBeStomped = true;
    [Tooltip("Upward bounce velocity given to the player on stomp")]
    [SerializeField] private float bounceForce = 6.5f;
    [Tooltip("Sound played when enemy is stomped")]
    [SerializeField] private AudioClip stompSound;
    [Tooltip("Time to wait before destroying GameObject (to let death animation play)")]
    [SerializeField] private float deathDelay = 0.5f;

    private Rigidbody2D _rb;
    private BoxCollider2D _col;
    private SpriteRenderer _sr;
    private Animator _anim;

    private Vector2 _startPosition;
    private float _limitNeg;
    private float _limitPos;
    private bool _movingPos = true;
    private bool _isDead = false;

    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<BoxCollider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();

        // Unity-physics: Kinematic body moved with MovePosition
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        _startPosition = transform.position;
        RecalcLimits();
    }

    private void RecalcLimits()
    {
        if (patrolAxis == PatrolAxis.Horizontal)
        {
            _limitNeg = _startPosition.x - leftDistance;
            _limitPos = _startPosition.x + rightDistance;
        }
        else
        {
            _limitNeg = _startPosition.y - leftDistance;   // down
            _limitPos = _startPosition.y + rightDistance;  // up
        }
    }

    private void Start()
    {
        if (patrolAxis == PatrolAxis.Horizontal)
        {
            UpdateFacing();
        }
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        float dir = _movingPos ? 1f : -1f;
        Vector2 next = _rb.position;

        if (patrolAxis == PatrolAxis.Horizontal)
        {
            next.x += dir * moveSpeed * Time.fixedDeltaTime;
            if (_movingPos && next.x >= _limitPos)
            {
                next.x = _limitPos;
                _movingPos = false;
                UpdateFacing();
            }
            else if (!_movingPos && next.x <= _limitNeg)
            {
                next.x = _limitNeg;
                _movingPos = true;
                UpdateFacing();
            }
        }
        else // Vertical
        {
            next.y += dir * moveSpeed * Time.fixedDeltaTime;
            if (_movingPos && next.y >= _limitPos)
            {
                next.y = _limitPos;
                _movingPos = false;
            }
            else if (!_movingPos && next.y <= _limitNeg)
            {
                next.y = _limitNeg;
                _movingPos = true;
            }
        }

        _rb.MovePosition(next);
    }

    private void UpdateFacing()
    {
        if (_sr != null && patrolAxis == PatrolAxis.Horizontal)
        {
            _sr.flipX = _movingPos;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;

        PayerController player = other.GetComponent<PayerController>()
                              ?? other.GetComponentInParent<PayerController>();

        if (player == null) return;

        Rigidbody2D playerRb = other.attachedRigidbody ?? player.GetComponent<Rigidbody2D>();

        // Stomp detection: check if player is falling onto the enemy head
        bool isStomping = false;
        if (canBeStomped)
        {
            float enemyTop = transform.position.y + (_col != null ? _col.offset.y + (_col.size.y * 0.15f) : 0.1f);
            bool playerIsAbove = other.bounds.min.y >= enemyTop || (player.transform.position.y > transform.position.y + 0.2f);
            bool playerIsFalling = playerRb == null || playerRb.linearVelocity.y <= 0.5f;

            if (playerIsAbove && playerIsFalling)
            {
                isStomping = true;
            }
        }

        if (isStomping)
        {
            // Bounce player upward
            player.Bounce(bounceForce);

            // Enemy dies
            Die();
        }
        else
        {
            // Player takes damage
            player.TakeDamage(1);
        }
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Disable collider & physics so it no longer hurts or interacts
        if (_col != null) _col.enabled = false;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;
        }

        // Play stomp sound
        if (stompSound != null)
        {
            AudioSource.PlayClipAtPoint(stompSound, transform.position);
        }

        // Play die animation (unity-animation)
        if (_anim != null)
        {
            _anim.SetTrigger(DieHash);
        }

        // Destroy after delay so the squashed/die animation is visible
        Destroy(gameObject, deathDelay);
    }

    private void OnDrawGizmos()
    {
        if (_isDead) return;

        Vector3 origin = Application.isPlaying ? (Vector3)_startPosition : transform.position;

        if (patrolAxis == PatrolAxis.Horizontal || !Application.isPlaying)
        {
            bool isVertical = (patrolAxis == PatrolAxis.Vertical);

            Vector3 negPoint = isVertical
                ? new Vector3(origin.x, origin.y - leftDistance, 0f)
                : new Vector3(origin.x - leftDistance, origin.y, 0f);

            Vector3 posPoint = isVertical
                ? new Vector3(origin.x, origin.y + rightDistance, 0f)
                : new Vector3(origin.x + rightDistance, origin.y, 0f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(negPoint, posPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(negPoint, 0.25f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(posPoint, 0.25f);
        }
        else
        {
            float downLimit = _limitNeg;
            float upLimit = _limitPos;

            Vector3 downPoint = new Vector3(origin.x, downLimit, 0f);
            Vector3 upPoint = new Vector3(origin.x, upLimit, 0f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(downPoint, upPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(downPoint, 0.25f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(upPoint, 0.25f);
        }
    }
}