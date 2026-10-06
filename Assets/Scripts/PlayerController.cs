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
    private float nextShotTime;
    private float rapidFireUntil;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        RotateToPointer();
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextShotTime)
        {
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = new Vector2(Keyboard.current.dKey.isPressed ? 1f : Keyboard.current.aKey.isPressed ? -1f : 0f,
                                    Keyboard.current.wKey.isPressed ? 1f : Keyboard.current.sKey.isPressed ? -1f : 0f);
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
        Vector3 cameraPosition = transform.position;
        cameraPosition.z = -10f;
        mainCamera.transform.position = cameraPosition;
    }

    private void Shoot()
    {
        nextShotTime = Time.time + fireCooldown;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation).Launch(firePoint.right);
    }
}