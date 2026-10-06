using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, 2f);
    }

    public void Launch(Vector2 direction)
    {
        body.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponent<Health>();
        if (health != null && other.GetComponent<ZombieAI>() != null)
        {
            health.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}