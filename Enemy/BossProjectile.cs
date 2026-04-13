using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private static Sprite sharedSprite;

    private Rigidbody2D rb;
    private CircleCollider2D projectileCollider;
    private SpriteRenderer spriteRenderer;
    private float damage;

    public void Initialize(Vector2 direction, float speed, float damageAmount, float lifetime, Color color)
    {
        EnsureComponents();

        damage = damageAmount;
        spriteRenderer.color = color;
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void EnsureComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        projectileCollider = GetComponent<CircleCollider2D>();
        if (projectileCollider == null)
        {
            projectileCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        projectileCollider.isTrigger = true;
        projectileCollider.radius = 0.2f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = GetSharedSprite();
        spriteRenderer.sortingOrder = 5;

        transform.localScale = Vector3.one * 0.35f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerController.Instance != null)
        {
            PlayerController.Instance.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private static Sprite GetSharedSprite()
    {
        if (sharedSprite != null)
        {
            return sharedSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        sharedSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            texture.width
        );

        return sharedSprite;
    }
}
