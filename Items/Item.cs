using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float stopDistance = 0.1f;
    private protected float playerYOffset = 0.5f; // Adjust to match player's feet/collider position

    protected Rigidbody2D rb;
    protected bool isMovingToPlayer = false;
    protected bool collected = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (isMovingToPlayer && !collected && PlayerController.Instance != null)
        {
            MoveTowardPlayer();
        }
    }

    public void OnPickedUp()
    {
        isMovingToPlayer = true;
    }

    private void MoveTowardPlayer()
    {
        // Target the player's collider position, not the pivot
        Vector3 playerPos = PlayerController.Instance.transform.position + new Vector3(0f, playerYOffset, 0f);
        Vector3 direction = (playerPos - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, playerPos);

        if (distance > stopDistance)
        {
            if (rb != null)
                rb.linearVelocity = direction * moveSpeed;
            else
                transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            AwardAndDestroy();
        }
    }

    // Each item type defines what happens when collected
    protected abstract void AwardAndDestroy();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other != null && other.CompareTag("Player"))
            AwardAndDestroy();

        if (other != null && other.GetComponent<ItemsPickUp>() != null)
            OnPickedUp();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collected) return;
        Collider2D other = collision.collider;

        if (other != null && other.CompareTag("Player"))
            AwardAndDestroy();

        if (other != null && other.GetComponent<ItemsPickUp>() != null)
            OnPickedUp();
    }
}
