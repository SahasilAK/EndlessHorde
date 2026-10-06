using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        RapidFire
    }

    [SerializeField] private PickupType type;
    [SerializeField] private int healAmount = 25;
    [SerializeField] private float rapidFireDuration = 6f;

    public void Configure(PickupType pickupType)
    {
        type = pickupType;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Sprite retroSprite = Resources.Load<Sprite>(type == PickupType.Health
            ? "RetroPixel/HealthPickup"
            : "RetroPixel/RapidFirePickup");
        if (retroSprite != null)
        {
            sprite.sprite = retroSprite;
            sprite.color = Color.white;
        }
        else
        {
            if (sprite.sprite == null)
            {
                sprite.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f));
            }
            sprite.color = type == PickupType.Health ? new Color(.2f, 1f, .3f) : new Color(1f, .75f, .15f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (type == PickupType.Health)
        {
            Health health = player.GetComponent<Health>();
            if (health == null)
            {
                return;
            }
            health.Heal(healAmount);
        }
        else
        {
            player.SetRapidFire(rapidFireDuration);
        }
        Destroy(gameObject);
    }
}