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
        GetComponent<SpriteRenderer>().color = type == PickupType.Health ? new Color(.2f, 1f, .3f) : new Color(1f, .75f, .15f);
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
            player.GetComponent<Health>().Heal(healAmount);
        }
        else
        {
            player.SetRapidFire(rapidFireDuration);
        }
        Destroy(gameObject);
    }
}