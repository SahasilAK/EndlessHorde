using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class PlayerController : MonoBehaviour
{
    public enum Upgrade
    {
        RapidFire,
        HeavyRounds,
        SwiftFeet,
        Vitality,
        TwinShot,
        SpreadShot,
        BurstFire,
        PiercingRounds,
        Regeneration,
        BossSlayer,
        VampiricRounds,
        QuickRecovery,
        Scavenger
    }

    private enum FireMode { Single, Twin, Spread, Burst }

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private float fireCooldown = 0.2f;
    [SerializeField] private float damageInvulnerabilityDuration = 1f;
    [SerializeField] private float regenerationInterval = 5f;

    private Rigidbody2D body;
    private Health health;
    private Camera mainCamera;
    private GameManager gameManager;
    private float nextShotTime;
    private float nextDamageTime;
    private float rapidFireUntil;
    private float cameraShakeRemaining;
    private FireMode fireMode;
    private int bulletDamage = 1;
    private int piercingRounds;
    private int bossDamageBonus;
    private int regenerationLevel;
    private float pickupDropBonus;
    private bool vampiricRounds;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        mainCamera = Camera.main;
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f || Mouse.current == null)
        {
            return;
        }

        RotateToPointer();
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextShotTime)
        {
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 input = new Vector2(keyboard.dKey.isPressed ? 1f : keyboard.aKey.isPressed ? -1f : 0f,
                                    keyboard.wKey.isPressed ? 1f : keyboard.sKey.isPressed ? -1f : 0f);
        body.linearVelocity = input.normalized * moveSpeed;
    }

    private void RotateToPointer()
    {
        Vector3 pointer = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (pointer - transform.position).normalized;
        transform.right = direction;
    }

    private void LateUpdate()
    {
        Vector2 shakeOffset = Vector2.zero;
        if (cameraShakeRemaining > 0f)
        {
            cameraShakeRemaining -= Time.unscaledDeltaTime;
            shakeOffset = Random.insideUnitCircle * cameraShakeRemaining;
        }

        Vector3 cameraPosition = transform.position;
        cameraPosition.z = -10f;
        cameraPosition.x += shakeOffset.x;
        cameraPosition.y += shakeOffset.y;
        mainCamera.transform.position = cameraPosition;
    }

    public void ShakeCamera()
    {
        cameraShakeRemaining = 0.12f;
    }

    public void TakeDamage(int amount)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        nextDamageTime = Time.time + damageInvulnerabilityDuration;
        health.TakeDamage(amount);
    }

    private void Shoot()
    {
        nextShotTime = Time.time + (Time.time < rapidFireUntil ? fireCooldown * 0.5f : fireCooldown);
        gameManager.PlayShootSound();
        Vector2 direction = firePoint.right;
        switch (fireMode)
        {
            case FireMode.Twin:
                Fire(direction, -7f);
                Fire(direction, 7f);
                break;
            case FireMode.Spread:
                for (int i = -2; i <= 2; i++)
                {
                    Fire(direction, i * 10f);
                }
                break;
            case FireMode.Burst:
                StartCoroutine(FireBurst(direction));
                break;
            default:
                Fire(direction, 0f);
                break;
        }
    }

    private IEnumerator FireBurst(Vector2 direction)
    {
        for (int i = 0; i < 3; i++)
        {
            Fire(direction, 0f);
            if (i < 2)
            {
                yield return new WaitForSeconds(0.08f);
            }
        }
    }

    private void Fire(Vector2 direction, float angle)
    {
        Vector2 shotDirection = Quaternion.Euler(0f, 0f, angle) * direction;
        Bullet bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Launch(shotDirection, bulletDamage, piercingRounds, bossDamageBonus, health, vampiricRounds);
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        switch (upgrade)
        {
            case Upgrade.RapidFire:
                fireCooldown = Mathf.Max(0.06f, fireCooldown * 0.8f);
                break;
            case Upgrade.HeavyRounds:
                bulletDamage++;
                break;
            case Upgrade.SwiftFeet:
                moveSpeed += 0.8f;
                break;
            case Upgrade.Vitality:
                health.SetMaxHealth(health.Max + 2);
                break;
            case Upgrade.TwinShot:
                fireMode = FireMode.Twin;
                break;
            case Upgrade.SpreadShot:
                fireMode = FireMode.Spread;
                break;
            case Upgrade.BurstFire:
                fireMode = FireMode.Burst;
                break;
            case Upgrade.PiercingRounds:
                piercingRounds++;
                break;
            case Upgrade.Regeneration:
                regenerationLevel++;
                if (regenerationLevel == 1)
                {
                    StartCoroutine(RegenerateHealth());
                }
                break;
            case Upgrade.BossSlayer:
                bossDamageBonus += 2;
                break;
            case Upgrade.VampiricRounds:
                vampiricRounds = true;
                break;
            case Upgrade.QuickRecovery:
                damageInvulnerabilityDuration += 0.35f;
                break;
            case Upgrade.Scavenger:
                pickupDropBonus += 0.1f;
                break;
        }
    }

    public static Upgrade[] RollUpgradeChoices(int count)
    {
        Upgrade[] upgrades = (Upgrade[])System.Enum.GetValues(typeof(Upgrade));
        int choiceCount = Mathf.Clamp(count, 0, upgrades.Length);
        for (int i = 0; i < choiceCount; i++)
        {
            int swapIndex = Random.Range(i, upgrades.Length);
            (upgrades[i], upgrades[swapIndex]) = (upgrades[swapIndex], upgrades[i]);
        }
        System.Array.Resize(ref upgrades, choiceCount);
        return upgrades;
    }

    public static string GetUpgradeTitle(Upgrade upgrade)
    {
        switch (upgrade)
        {
            case Upgrade.RapidFire: return "Rapid Fire";
            case Upgrade.HeavyRounds: return "Heavy Rounds";
            case Upgrade.SwiftFeet: return "Swift Feet";
            case Upgrade.Vitality: return "Vitality";
            case Upgrade.TwinShot: return "Twin Shot";
            case Upgrade.SpreadShot: return "Spread Shot";
            case Upgrade.BurstFire: return "Burst Fire";
            case Upgrade.PiercingRounds: return "Piercing Rounds";
            case Upgrade.Regeneration: return "Regeneration";
            case Upgrade.BossSlayer: return "Boss Slayer";
            case Upgrade.VampiricRounds: return "Vampiric Rounds";
            case Upgrade.QuickRecovery: return "Quick Recovery";
            default: return "Scavenger";
        }
    }

    public static string GetUpgradeDescription(Upgrade upgrade)
    {
        switch (upgrade)
        {
            case Upgrade.RapidFire: return "Fire 20% faster";
            case Upgrade.HeavyRounds: return "Deal +1 damage per bullet";
            case Upgrade.SwiftFeet: return "Move 0.8 units faster";
            case Upgrade.Vitality: return "+2 max health and heal";
            case Upgrade.TwinShot: return "Fire two angled bullets";
            case Upgrade.SpreadShot: return "Fire five bullets in a spread";
            case Upgrade.BurstFire: return "Fire three quick bullets";
            case Upgrade.PiercingRounds: return "Bullets pass through one more zombie";
            case Upgrade.Regeneration: return "Regenerate 1 health every 5 seconds";
            case Upgrade.BossSlayer: return "Deal +2 damage to bosses";
            case Upgrade.VampiricRounds: return "Heal 1 health for each kill";
            case Upgrade.QuickRecovery: return "Gain 0.35 seconds of damage protection";
            default: return "Increase zombie pickup drops by 10%";
        }
    }

    public float PickupDropBonus => pickupDropBonus;

    private IEnumerator RegenerateHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenerationInterval);
            health.Heal(regenerationLevel);
        }
    }

    public void SetRapidFire(float duration)
    {
        rapidFireUntil = Mathf.Max(rapidFireUntil, Time.time + duration);
    }
}