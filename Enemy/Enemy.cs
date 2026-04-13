using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected NavMeshAgent agent;
    protected Vector3 direction;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected GameObject destroyEffect;
    [SerializeField] protected GameObject ExpCrystalPrefab;
    [SerializeField] protected GameObject coinPrefab;
    [SerializeField] protected int coinValue = 1;
    [SerializeField] [Range(0f, 1f)] protected float coinDropChance = 0.1f;
    [SerializeField] protected GameObject expGatherAllPrefab;
    [SerializeField] [Range(0f, 1f)] protected float expGatherAllDropChance = 0.03f;
    [SerializeField] protected GameObject healthItemPrefab;
    [SerializeField] [Range(0f, 1f)] protected float healthItemDropChance = 0.03f;
    [SerializeField] protected float damage;
    [SerializeField] protected float health;
    [SerializeField] protected int experienceToGive;
    [SerializeField] protected int pointsValue; // Points awarded for killing this enemy
    [SerializeField] protected float pushTime;
    protected float pushCounter;

    [Tooltip("Enable if the sprite faces right by default")]
    [SerializeField] protected bool isFacingRightDefault = false;

    // Base stats for scaling
    protected float baseHealth;
    protected float baseDamage;
    protected float baseMoveSpeed;
    protected int baseExperience;
    protected int basePoints;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Store base stats when enemy is created
        baseHealth = health;
        baseDamage = damage;
        baseMoveSpeed = moveSpeed;
        baseExperience = experienceToGive;
        basePoints = pointsValue;
    }

    protected virtual void Start()
    {
        // Get NavMeshAgent if not assigned
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        if (agent != null)
        {
            // Configure NavMeshAgent for 2D (XY plane)
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = moveSpeed;
        }
    }

    public void ScaleStats(float healthMultiplier, float damageMultiplier, float speedMultiplier, float expMultiplier)
    {
        health = baseHealth * healthMultiplier;
        damage = baseDamage * damageMultiplier;
        moveSpeed = baseMoveSpeed * speedMultiplier;
        experienceToGive = Mathf.RoundToInt(baseExperience * expMultiplier);
        pointsValue = Mathf.RoundToInt(basePoints * expMultiplier);
        
        // Update NavMeshAgent speed
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeSelf == true)
        {   
            // Flip enemy to face player
            bool isPlayerRight = PlayerController.Instance.transform.position.x > transform.position.x;
            spriteRenderer.flipX = isFacingRightDefault ? !isPlayerRight : isPlayerRight;

            direction = (PlayerController.Instance.transform.position - transform.position).normalized;

            //push back
            if (pushCounter > 0) 
            { 
                pushCounter -= Time.deltaTime;

                // Pause agent during pushback and use rigidbody for knockback force
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(-direction.x * Mathf.Abs(moveSpeed), -direction.y * Mathf.Abs(moveSpeed));
                }

                if (pushCounter <= 0) 
                { 
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                }

                return;
            }

            // Move towards player using NavMeshAgent
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.speed = moveSpeed;
                agent.SetDestination(PlayerController.Instance.transform.position);

                // Zero out rigidbody so it doesn't conflict with the agent
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
            else if (rb != null)
            {
                // Fallback to rigidbody movement if no agent
                rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
            }
        }
        else
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player") && PlayerController.Instance != null) {
            PlayerController.Instance.TakeDamage(damage);
  
        }
    }
    
    public virtual void TakeDamage(float damage)
    {
        float multiplier = PlayerController.Instance != null ? PlayerController.Instance.damageMultiplier : 1f;
        health -= damage * multiplier;
        if (DamageNumberController.Instance != null)
        {
            DamageNumberController.Instance.CreateNumber(damage, transform.position);
        }

        pushCounter = pushTime;
        if (health <= 0)
        {
            SpawnDeathRewards();
            Destroy(gameObject);
        }
    }

    protected void SpawnDeathRewards()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        if (ExpCrystalPrefab != null)
        {
            GameObject crystal = Instantiate(ExpCrystalPrefab, transform.position, Quaternion.identity);
            ExpCrystal expCrystal = crystal.GetComponent<ExpCrystal>();
            if (expCrystal != null)
            {
                expCrystal.SetXPValue(experienceToGive);
            }
        }

        // 10% chance to drop a coin by default — adjustable per enemy in Inspector
        if (coinPrefab != null)
        {
            // Gold Fever = 100% drop, otherwise use normal chance
            float chance = coinDropChance;
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null && spawner.IsGoldFeverActive)
                chance = 1f;

            if (Random.value <= chance)
            {
                GameObject coinObj = Instantiate(coinPrefab, transform.position, Quaternion.identity);
                Coin coin = coinObj.GetComponent<Coin>();
                if (coin != null)
                    coin.SetCoinValue(coinValue);
            }
        }

        if (expGatherAllPrefab != null && Random.value <= expGatherAllDropChance)
        {
            Instantiate(expGatherAllPrefab, transform.position, Quaternion.identity);
        }

        if (healthItemPrefab != null && Random.value <= healthItemDropChance)
        {
            Instantiate(healthItemPrefab, transform.position, Quaternion.identity);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
            GameManager.Instance.AddPoints(pointsValue);
        }
    }
}


