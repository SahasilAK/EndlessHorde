using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(SpriteRenderer))]
public class ZombieAI : MonoBehaviour
{
    public enum Variant
    {
        Normal,
        FastWeak,
        SlowTough
    }

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int touchDamage = 10;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField, Range(0f, 1f)] private float pickupDropChance = 0.25f;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite fastSprite;
    [SerializeField] private Sprite toughSprite;

    private Rigidbody2D body;
    private Health health;
    private GameManager gameManager;
    private float nextDamageTime;
    private float speedMultiplier = 1f;
    private Color variantTint = Color.white;
    private Coroutine damageFlash;
    private int previousHealth;

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void ConfigureVariant(Variant variant)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        switch (variant)
        {
            case Variant.FastWeak:
                moveSpeed = 3.4f;
                health.SetMaxHealth(1);
                variantTint = new Color(.55f, 1f, .4f);
                if (fastSprite != null) sprite.sprite = fastSprite;
                break;
            case Variant.SlowTough:
                moveSpeed = 1.2f;
                health.SetMaxHealth(7);
                variantTint = new Color(.65f, .55f, .9f);
                if (toughSprite != null) sprite.sprite = toughSprite;
                break;
            default:
                moveSpeed = 2f;
                health.SetMaxHealth(3);
                variantTint = Color.white;
                if (normalSprite != null) sprite.sprite = normalSprite;
                break;
        }
        sprite.color = variantTint;
        previousHealth = health.Current;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        previousHealth = health.Current;
        health.Died += OnDied;
        health.Changed += OnHealthChanged;
    }

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
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
        transform.up = direction;
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
            gameManager.PlayZombieDeathSound();
        }
        if (Random.value < pickupDropChance)
        {
            DropPickup();
        }
        Destroy(gameObject);
    }

    private void OnHealthChanged(Health changedHealth)
    {
        if (changedHealth.Current < previousHealth && !changedHealth.IsDead)
        {
            gameManager.PlayHitSound();
            if (damageFlash != null)
            {
                StopCoroutine(damageFlash);
            }
            damageFlash = StartCoroutine(FlashOnDamage());
        }
        previousHealth = changedHealth.Current;
    }

    private IEnumerator FlashOnDamage()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = Color.white;
        yield return new WaitForSecondsRealtime(.08f);
        if (sprite != null)
        {
            sprite.color = variantTint;
        }
        damageFlash = null;
    }

    private void DropPickup()
    {
        GameObject pickupObject = new GameObject("Pickup");
        pickupObject.transform.position = transform.position;
        pickupObject.transform.localScale = Vector3.one * .45f;
        pickupObject.AddComponent<SpriteRenderer>();
        CircleCollider2D collider = pickupObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        Pickup pickup = pickupObject.AddComponent<Pickup>();
        pickup.Configure(Random.value < .5f ? Pickup.PickupType.Health : Pickup.PickupType.RapidFire);
    }
}