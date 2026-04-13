using System.Collections;
using UnityEngine;

public class BossController : Enemy
{
    [SerializeField] private Animator animator;

    [Header("Boss Movement")]
    [SerializeField] private float chaseStopDistance = 6f;
    [SerializeField] private float retreatDistance = 3.25f;

    [Header("Boss Ranged Attack")]
    [SerializeField] private BossProjectile projectilePrefab;
    [SerializeField] private float rangedAttackCooldown = 3f;
    [SerializeField] private float attackWindup = 0.45f;
    [SerializeField] private int projectilesPerBurst = 5;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileLifetime = 4f;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float projectileSpread = 55f;
    [SerializeField] private Vector2 projectileOffset = new Vector2(0f, 1.5f);
    [SerializeField] private Color projectileColor = new Color(1f, 0.45f, 0.2f, 1f);

    [Header("Boss Phases")]
    [SerializeField] private float armorTriggerHealthPercent = 0.65f;
    [SerializeField] private float armorDuration = 4f;
    [SerializeField] private float armorDamageReduction = 0.45f;
    [SerializeField] private float armorMoveSpeedMultiplier = 1.2f;
    [SerializeField] private float immuneTriggerHealthPercent = 0.3f;
    [SerializeField] private float immuneDurationSeconds = 2f;
    [SerializeField] private float defeatDelay = 1.25f;
    [SerializeField] private Color immuneTextColor = new Color(0.45f, 0.95f, 1f, 1f);

    private bool armorPhaseTriggered;
    private bool armorBuffActive;
    private bool immunePhaseTriggered;
    private bool isImmunePhase;
    private bool isAttacking;
    private bool defeated;
    private float rangedAttackTimer;
    private float defaultMoveSpeed;

    protected override void Awake()
    {
        ApplyDefaultStats();
        base.Awake();
        EnsureRuntimeComponents();

        defaultMoveSpeed = moveSpeed;
        rangedAttackTimer = rangedAttackCooldown * 0.5f;
    }

    private void Start()
    {
        SetLoopAnimationForCurrentState();
    }

    protected override void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (defeated || PlayerController.Instance == null || !PlayerController.Instance.gameObject.activeSelf)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        UpdateFacing();

        if (pushCounter > 0)
        {
            pushCounter -= Time.deltaTime;
            if (moveSpeed > 0)
            {
                moveSpeed = -moveSpeed;
            }

            if (pushCounter <= 0)
            {
                moveSpeed = Mathf.Abs(defaultMoveSpeed);
            }
        }
        else
        {
            moveSpeed = armorBuffActive ? defaultMoveSpeed * armorMoveSpeedMultiplier : defaultMoveSpeed;
        }

        rangedAttackTimer -= Time.deltaTime;

        if (isAttacking || isImmunePhase)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = PlayerController.Instance.transform.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        Vector2 moveDirection = Vector2.zero;

        if (distanceToPlayer > chaseStopDistance)
        {
            moveDirection = toPlayer.normalized;
        }
        else if (distanceToPlayer < retreatDistance)
        {
            moveDirection = -toPlayer.normalized;
        }

        rb.linearVelocity = moveDirection * moveSpeed;

        if (rangedAttackTimer <= 0f)
        {
            StartCoroutine(RangedAttackRoutine());
        }
    }

    public override void TakeDamage(float incomingDamage)
    {
        if (defeated)
        {
            return;
        }

        if (isImmunePhase)
        {
            ShowImmuneText();
            return;
        }

        float damageToApply = incomingDamage;
        if (armorBuffActive)
        {
            damageToApply *= 1f - Mathf.Clamp01(armorDamageReduction);
        }

        health -= damageToApply;

        if (DamageNumberController.Instance != null)
        {
            DamageNumberController.Instance.CreateNumber(damageToApply, transform.position + Vector3.up * 2f);
        }

        pushCounter = pushTime;

        if (health <= 0f)
        {
            StartCoroutine(DefeatRoutine());
            return;
        }

        if (!armorPhaseTriggered && health <= baseHealth * armorTriggerHealthPercent)
        {
            StartCoroutine(ArmorBuffRoutine());
        }

        if (!immunePhaseTriggered && health <= baseHealth * immuneTriggerHealthPercent)
        {
            StartCoroutine(ImmuneRoutine());
        }
    }

    private void ApplyDefaultStats()
    {
        if (moveSpeed <= 0f)
        {
            moveSpeed = 1.8f;
        }

        if (damage <= 0f)
        {
            damage = 18f;
        }

        if (health <= 0f)
        {
            health = 300f;
        }

        if (experienceToGive <= 0)
        {
            experienceToGive = 150;
        }

        if (pointsValue <= 0)
        {
            pointsValue = 750;
        }

        if (pushTime <= 0f)
        {
            pushTime = 0.08f;
        }
    }

    private void EnsureRuntimeComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

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

        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider2D>();
        }

        capsuleCollider.direction = CapsuleDirection2D.Vertical;
        capsuleCollider.offset = new Vector2(-0.1f, 1.85f);
        capsuleCollider.size = new Vector2(4.95f, 5.15f);

        gameObject.tag = "Enemies";
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null || PlayerController.Instance == null)
        {
            return;
        }

        spriteRenderer.flipX = PlayerController.Instance.transform.position.x < transform.position.x;
    }

    private IEnumerator RangedAttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        PlayAnimation("BossRangeAtk");

        yield return new WaitForSeconds(attackWindup);
        SpawnProjectileBurst();

        yield return new WaitForSeconds(0.35f);
        rangedAttackTimer = rangedAttackCooldown;
        isAttacking = false;
        SetLoopAnimationForCurrentState();
    }

    private IEnumerator ArmorBuffRoutine()
    {
        armorPhaseTriggered = true;
        armorBuffActive = true;
        SetLoopAnimationForCurrentState();

        yield return new WaitForSeconds(armorDuration);

        armorBuffActive = false;
        SetLoopAnimationForCurrentState();
    }

    private IEnumerator ImmuneRoutine()
    {
        immunePhaseTriggered = true;
        isImmunePhase = true;
        rb.linearVelocity = Vector2.zero;
        SetLoopAnimationForCurrentState();

        yield return new WaitForSeconds(immuneDurationSeconds);

        isImmunePhase = false;
        rangedAttackTimer = Mathf.Min(rangedAttackTimer, 1f);
        SetLoopAnimationForCurrentState();
    }

    private IEnumerator DefeatRoutine()
    {
        if (defeated)
        {
            yield break;
        }

        defeated = true;
        health = 0f;
        rb.linearVelocity = Vector2.zero;

        Collider2D bossCollider = GetComponent<Collider2D>();
        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        PlayAnimation("BossDefeated");

        yield return new WaitForSeconds(defeatDelay);

        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
            GameManager.Instance.AddPoints(pointsValue);
        }

        Destroy(gameObject);
    }

    private void SpawnProjectileBurst()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        Vector2 spawnPosition = (Vector2)transform.position + projectileOffset;
        Vector2 baseDirection = ((Vector2)PlayerController.Instance.transform.position - spawnPosition).normalized;
        float spreadStep = projectilesPerBurst > 1 ? projectileSpread / (projectilesPerBurst - 1) : 0f;
        float startingAngle = -projectileSpread * 0.5f;

        for (int i = 0; i < projectilesPerBurst; i++)
        {
            float currentAngle = startingAngle + (spreadStep * i);
            Vector2 direction = Rotate(baseDirection, currentAngle);
            BossProjectile projectile = CreateProjectileInstance(spawnPosition);
            projectile.Initialize(direction, projectileSpeed, projectileDamage, projectileLifetime, projectileColor);
        }
    }

    private BossProjectile CreateProjectileInstance(Vector2 spawnPosition)
    {
        if (projectilePrefab != null)
        {
            return Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        }

        GameObject projectileObject = new GameObject("BossProjectile");
        projectileObject.transform.position = spawnPosition;
        return projectileObject.AddComponent<BossProjectile>();
    }

    private void ShowImmuneText()
    {
        if (DamageNumberController.Instance != null)
        {
            DamageNumberController.Instance.CreateText("IMMUNE", transform.position + Vector3.up * 3f, immuneTextColor);
        }
    }

    private void SetLoopAnimationForCurrentState()
    {
        if (defeated)
        {
            PlayAnimation("BossDefeated");
        }
        else if (isImmunePhase)
        {
            PlayAnimation("BossImmune");
        }
        else if (armorBuffActive)
        {
            PlayAnimation("BossArmorBuff");
        }
        else
        {
            PlayAnimation("BossIdle");
        }
    }

    private void PlayAnimation(string stateName)
    {
        if (animator != null)
        {
            animator.Play(stateName, 0, 0f);
        }
    }

    private Vector2 Rotate(Vector2 vector, float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            (cos * vector.x) - (sin * vector.y),
            (sin * vector.x) + (cos * vector.y)
        );
    }
}
