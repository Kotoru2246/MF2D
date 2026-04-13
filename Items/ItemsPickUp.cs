using UnityEngine;

public class ItemsPickUp : MonoBehaviour
{
    [SerializeField] private string itemTag = "Items";

    private CircleCollider2D pickupCollider;
    private float baseRadius;

    private void Awake()
    {
        pickupCollider = GetComponent<CircleCollider2D>();
        if (pickupCollider != null) baseRadius = pickupCollider.radius;
    }

    public void SetPickupRadius(float bonusRadius)
    {
        if (pickupCollider != null)
            pickupCollider.radius = baseRadius + bonusRadius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag(itemTag))
        {
            // Check if it's an ExpCrystal
            ExpCrystal crystal = other.GetComponent<ExpCrystal>();
            if (crystal != null)
            {
                // Trigger pickup behavior instead of destroying
                crystal.OnPickedUp();
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (other != null && other.CompareTag(itemTag))
        {
            // Check if it's an ExpCrystal
            ExpCrystal crystal = other.GetComponent<ExpCrystal>();
            if (crystal != null)
            {
                // Trigger pickup behavior instead of destroying
                crystal.OnPickedUp();
            }
            
        }
    }
}
