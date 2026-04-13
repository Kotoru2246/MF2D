using UnityEngine;

public class ReaperSlashWave : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 12f;
    [SerializeField] private float lifetime = 2.5f;

    private Vector2 moveDirection = Vector2.right;

    private void Awake()
    {
        EnsureComponents();
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    public void Initialize(Vector2 direction, float projectileSpeed, float projectileDamage, float projectileLifetime)
    {
        EnsureComponents();

        moveDirection = direction.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        lifetime = projectileLifetime;

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = moveDirection.x < 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerController.Instance != null)
        {
            PlayerController.Instance.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void EnsureComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider2D>();
        }

        if (hitCollider == null)
        {
            hitCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        hitCollider.isTrigger = true;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
