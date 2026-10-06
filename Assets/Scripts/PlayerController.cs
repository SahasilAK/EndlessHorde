using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private float fireCooldown = 0.2f;

    private Rigidbody2D body;
    private Camera mainCamera;
    private GameManager gameManager;
    private float nextShotTime;
    private float rapidFireUntil;
    private float cameraShakeRemaining;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
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

    private void Shoot()
    {
        float cooldown = Time.time < rapidFireUntil ? fireCooldown * 0.5f : fireCooldown;
        nextShotTime = Time.time + cooldown;
        gameManager.PlayShootSound();
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation).Launch(firePoint.right);
    }

    public void SetRapidFire(float duration)
    {
        rapidFireUntil = Mathf.Max(rapidFireUntil, Time.time + duration);
    }
}