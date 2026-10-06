using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(SpriteRenderer))]
public class ZombieAI : MonoBehaviour
{
    private const float HealthBarWidth = 0.4f;
    private const float HealthBarHeight = 0.05f;
    private static Sprite healthBarSprite;

    public enum Variant
    {
        Normal,
        FastWeak,
        SlowTough,
        Boss
    }

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int touchDamage = 1;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField, Range(0f, 1f)] private float pickupDropChance = 0.25f;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite fastSprite;
    [SerializeField] private Sprite toughSprite;
    [SerializeField] private Sprite bossSprite;

    public bool IsBoss { get; private set; }

    private Rigidbody2D body;
    private Health health;
    private GameManager gameManager;
    private float nextDamageTime;
    private float speedMultiplier = 1f;
    private Color variantTint = Color.white;
    private Coroutine damageFlash;
    private int previousHealth;
    private Transform healthBar;
    private Transform healthBarFill;

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
            case Variant.Boss:
                IsBoss = true;
                moveSpeed = 0.8f;
                touchDamage = 2;
                health.SetMaxHealth(30);
                variantTint = new Color(1f, .3f, .25f);
                if (bossSprite != null) sprite.sprite = bossSprite;
                else if (toughSprite != null) sprite.sprite = toughSprite;
                transform.localScale = Vector3.one * 2f;
                break;
            default:
                IsBoss = false;
                moveSpeed = 2f;
                touchDamage = 1;
                health.SetMaxHealth(3);
                variantTint = Color.white;
                if (normalSprite != null) sprite.sprite = normalSprite;
                break;
        }
        sprite.color = variantTint;
        previousHealth = health.Current;
        UpdateHealthBar();
    }

    public void ConfigureBoss(int wave)
    {
        ConfigureVariant(Variant.Boss);
        health.SetMaxHealth(25 + wave * 10);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        gameManager = FindAnyObjectByType<GameManager>();
        previousHealth = health.Current;
        health.Died += OnDied;
        health.Changed += OnHealthChanged;
        CreateHealthBar();
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

    private void LateUpdate()
    {
        if (healthBar != null)
        {
            healthBar.position = transform.position + Vector3.up * 0.22f;
            healthBar.rotation = Quaternion.identity;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player != null)
        {
            nextDamageTime = Time.time + damageCooldown;
            player.TakeDamage(touchDamage);
        }
    }

    private void OnDied(Health deadHealth)
    {
        if (gameManager != null)
        {
            gameManager.AddScore(10);
            gameManager.PlayZombieDeathSound();
        }
        float dropChance = pickupDropChance;
        if (gameManager != null && gameManager.Player != null)
        {
            PlayerController player = gameManager.Player.GetComponent<PlayerController>();
            if (player != null)
            {
                dropChance += player.PickupDropBonus;
            }
        }
        if (Random.value < Mathf.Clamp01(dropChance))
        {
            DropPickup();
        }
        Destroy(gameObject);
    }

    private void OnHealthChanged(Health changedHealth)
    {
        UpdateHealthBar();
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

    private void CreateHealthBar()
    {
        if (healthBarSprite == null)
        {
            healthBarSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        GameObject bar = new GameObject("Health Bar");
        healthBar = bar.transform;
        healthBar.SetParent(transform, false);

        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBar, false);
        background.transform.localScale = new Vector3(HealthBarWidth, HealthBarHeight, 1f);
        SpriteRenderer backgroundRenderer = background.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = healthBarSprite;
        backgroundRenderer.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        backgroundRenderer.sortingOrder = 5;

        GameObject fill = new GameObject("Fill");
        healthBarFill = fill.transform;
        healthBarFill.SetParent(healthBar, false);
        SpriteRenderer fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = healthBarSprite;
        fillRenderer.sortingOrder = 6;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null)
        {
            return;
        }

        float healthRatio = Mathf.Clamp01((float)health.Current / health.Max);
        float fillWidth = (HealthBarWidth - 0.02f) * healthRatio;
        healthBarFill.localScale = new Vector3(fillWidth, HealthBarHeight * 0.6f, 1f);
        healthBarFill.localPosition = new Vector3(-HealthBarWidth * 0.5f + 0.01f + fillWidth * 0.5f, 0f, 0f);
        healthBarFill.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, healthRatio);
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