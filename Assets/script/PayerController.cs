
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Animator))]
public class PayerController : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");

    [SerializeField] string playerName = "Alien";

    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip coinSound;
    [SerializeField] AudioClip hurtSound;
    AudioSource audioSource;
    Animator animator;

    [Range(1,100)]
    [SerializeField] float moveSpeed = 5.5f;
    [SerializeField] float jumpForce = 5.5f;
    int totalcoin = 5;
    int coins;

    private int maxLives = 3;
    private int currentLives = 3;
    private bool isInvulnerable = false;
    private bool canControl = true;
    private Dictionary<KeyColor, int> keys = new Dictionary<KeyColor, int>();

    bool isGrounded = true;
    Rigidbody2D rb;
    SpriteRenderer sprites;
    Vector2 move;

    float fallLimit = -6.0f;
    [SerializeField] TMP_Text coinText;
    [SerializeField] TMP_Text debugText;
    Vector3 startPosition;
    string bar;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprites = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLives = maxLives;
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateLives(currentLives, maxLives);
            GameUI.Instance.UpdateKeys(keys);
        }
        UpdateCoinText();
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < fallLimit)
        {
            FallRespawn();
        }
        Texts();
        bar = "";

        if (!canControl || (GameUI.Instance != null && GameUI.Instance.IsPaused))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
       
        move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        if (InputSystem.actions.FindAction("Jump").WasPressedThisFrame() && isGrounded)
        {
           rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
           isGrounded = false;
           PlaySounds(jumpSound);
        }
        
        if (move.x > 0.1f)
        {
            sprites.flipX = false;
        }
        else if (move.x < -0.1f)
        {
            sprites.flipX = true;
        }

        bool isWalking = Mathf.Abs(move.x) > 0.1f;
        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, isWalking);
            animator.SetBool(IsGroundedHash, isGrounded);
        }
        
    }
    // 50hz update
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(move.x * moveSpeed, rb.linearVelocity.y);
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("coin"))
        {
            coins += 1;
            UpdateCoinText();
            Destroy(other.gameObject);
            PlaySounds(coinSound);
        }
    }
    void UpdateCoinText()
    {
        coinText.text = $"coin : {coins} / {totalcoin}";
        if (coins == totalcoin)
        {
            coinText.text = "All coin collected";
        }
    }
    void PlaySounds(AudioClip audio)
    {
        audioSource.PlayOneShot(audio);
    }
    public void SetCanControl(bool val)
    {
        canControl = val;
    }

    public void AddKey(KeyColor color)
    {
        if (!keys.ContainsKey(color))
            keys[color] = 0;
        keys[color]++;

        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateKeys(keys);
        }
    }

    public int GetKeyCount(KeyColor color)
    {
        return keys.ContainsKey(color) ? keys[color] : 0;
    }

    public void UseKeys(KeyColor color, int count)
    {
        if (keys.ContainsKey(color))
        {
            keys[color] = Mathf.Max(0, keys[color] - count);
            if (GameUI.Instance != null)
            {
                GameUI.Instance.UpdateKeys(keys);
            }
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isInvulnerable || currentLives <= 0) return;

        currentLives -= damage;
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateLives(currentLives, maxLives);
        }

        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (animator != null)
        {
            animator.SetTrigger(HurtHash);
        }

        if (currentLives <= 0)
        {
            canControl = false;
            rb.linearVelocity = Vector2.zero;
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowEndScreen("Game Over", "Retry");
            }
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;
        float duration = 1.0f;
        float interval = 0.1f;

        while (elapsed < duration)
        {
            if (sprites != null)
            {
                sprites.enabled = !sprites.enabled;
            }
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        if (sprites != null)
        {
            sprites.enabled = true;
        }
        isInvulnerable = false;
    }

    private void FallRespawn()
    {
        currentLives--;
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateLives(currentLives, maxLives);
        }

        if (currentLives <= 0)
        {
            canControl = false;
            rb.linearVelocity = Vector2.zero;
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowEndScreen("Game Over", "Retry");
            }
        }
        else
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("fall off, reset to start");

    }
    void Texts()
    {
        debugText.text = "position : ("
            + transform.position.x.ToString("F2") + ","
            + transform.position.y.ToString("F2") + ",)" +
            $"Coin: {bar}";
    }
}
