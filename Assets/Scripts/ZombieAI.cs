using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class ZombieAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int touchDamage = 10;
    [SerializeField] private float damageCooldown = 1f;

    private Rigidbody2D body;
    private Health health;
    private GameManager gameManager;
    private float nextDamageTime;
    private float speedMultiplier = 1f;

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        health.Died += OnDied;
    }

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void FixedUpdate()
    {
        if (gameManager == null || gameManager.IsGameOver || gameManager.Player == null)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (gameManager.Player.position - transform.position).normalized;
        body.linearVelocity = direction * moveSpeed * speedMultiplier;
        transform.right = direction;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        Health playerHealth = collision.collider.GetComponent<Health>();
        if (playerHealth != null && collision.collider.GetComponent<PlayerController>() != null)
        {
            nextDamageTime = Time.time + damageCooldown;
            playerHealth.TakeDamage(touchDamage);
        }
    }

    private void OnDied(Health deadHealth)
    {
        if (gameManager != null)
        {
            gameManager.AddScore(10);
        }
        Destroy(gameObject);
    }
}