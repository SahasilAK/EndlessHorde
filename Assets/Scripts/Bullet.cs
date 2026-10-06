using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    private Rigidbody2D body;
    private int damage;
    private int piercesRemaining;
    private int bossDamageBonus;
    private Health ownerHealth;
    private bool healsOnKill;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, 2f);
    }

    public void Launch(Vector2 direction, int damage = 1, int pierces = 0, int bossBonus = 0, Health owner = null, bool healOnKill = false)
    {
        this.damage = damage;
        piercesRemaining = pierces;
        bossDamageBonus = bossBonus;
        ownerHealth = owner;
        healsOnKill = healOnKill;
        body.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health targetHealth = other.GetComponent<Health>();
        ZombieAI zombie = other.GetComponent<ZombieAI>();
        if (targetHealth != null && zombie != null)
        {
            bool wasAlive = !targetHealth.IsDead;
            targetHealth.TakeDamage(damage + (zombie.IsBoss ? bossDamageBonus : 0));
            if (wasAlive && targetHealth.IsDead && healsOnKill && ownerHealth != null)
            {
                ownerHealth.Heal(1);
            }

            if (piercesRemaining > 0)
            {
                piercesRemaining--;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}