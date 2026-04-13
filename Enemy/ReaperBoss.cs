using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaperBoss : Enemy
{
    [SerializeField] private Animator animator;

    [Header("Animation States")]
    [SerializeField] private string moveStateName = "ReaperIdle";
    [SerializeField] private string attackStateName = "ReaperAttack";
    [SerializeField] private string summonStateName = "ReaperSummon";
    [SerializeField] private string skillStateName = "ReaperSkill";
    [SerializeField] private string deathStateName = "ReaperDie";

    [Header("Attack")]
    [SerializeField] private ReaperSlashWave slashWavePrefab;
    [SerializeField] private Transform slashSpawnPoint;
    [SerializeField] private float slashWaveSpeed = 9f;
    [SerializeField] private float slashWaveDamage = 15f;
    [SerializeField] private float slashWaveLifetime = 2.5f;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float attackCooldown = 2.25f;
    [SerializeField] private float attackCastDelay = 0.45f;
    [SerializeField] private float attackAnimationDuration = 0.95f;

    [Header("Summon")]
    [SerializeField] private ReaperMiniBoss reaperMiniPrefab;
    [SerializeField] private Transform summonPoint;
    [SerializeField] private float summonCooldown = 6f;
    [SerializeField] private float summonCastDelay = 0.55f;
    [SerializeField] private float summonAnimationDuration = 1.1f;
    [SerializeField] private int maxActiveSummons = 3;
    [SerializeField] private float summonSpacing = 1.35f;

    [Header("Death")]
    [SerializeField] private float deathAnimationDuration = 1.2f;

    [Header("Skill")]
    [SerializeField] private float skillCooldown = 5f;
    [SerializeField] private float skillCastDelay = 0.45f;
    [SerializeField] private float skillAnimationDuration = 0.95f;
    [SerializeField] private float skillMinimumDistanceFromPlayer = 2.5f;
    [SerializeField] private float skillPreferredDistanceFromPlayer = 3.5f;

    private readonly List<ReaperMiniBoss> activeSummons = new List<ReaperMiniBoss>();
    private bool defeated;
    private bool isCasting;
    private float attackTimer;
    private float summonTimer;
    private float skillTimer;

    protected override void Awake()
    {
        base.Awake();
        isFacingRightDefault = true;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    protected override void Start()
    {
        base.Start();
        PlayState(moveStateName);
        attackTimer = attackCooldown * 0.5f;
        summonTimer = summonCooldown;
        skillTimer = skillCooldown;
    }

    protected override void FixedUpdate()
    {
        if (defeated || PlayerController.Instance == null || !PlayerController.Instance.gameObject.activeSelf)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            return;
        }

        CleanupSummons();

        if (isCasting)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // ép hướng ngay cả khi đang cast
            ForceFacePlayer();

            return;
        }

        attackTimer -= Time.deltaTime;
        summonTimer -= Time.deltaTime;
        skillTimer -= Time.deltaTime;

        base.FixedUpdate();

        PlayState(moveStateName);

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        if (CanUseSkill(distanceToPlayer))
        {
            StartCoroutine(SkillRoutine());
            return;
        }

        if (CanSummon())
        {
            StartCoroutine(SummonRoutine());
            return;
        }

        if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    public override void TakeDamage(float damageAmount)
    {
        if (defeated)
        {
            return;
        }

        health -= damageAmount;

        if (DamageNumberController.Instance != null)
        {
            DamageNumberController.Instance.CreateNumber(damageAmount, transform.position + Vector3.up * 2f);
        }

        pushCounter = pushTime;
        if (health <= 0f)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isCasting = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Attack thì boss nhìn về player
        ForceFacePlayer();

        PlayState(attackStateName);
        yield return new WaitForSeconds(attackCastDelay);

        SpawnSlashWave();
        PlayOptionalSkillState();

        yield return new WaitForSeconds(Mathf.Max(0f, attackAnimationDuration - attackCastDelay));

        attackTimer = attackCooldown;
        isCasting = false;
        PlayState(moveStateName);
    }

    private IEnumerator SkillRoutine()
    {
        isCasting = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Skill thì boss nhìn về player
        ForceFacePlayer();
  
        PlayState(skillStateName);
        yield return new WaitForSeconds(skillCastDelay);

        TeleportTowardPlayerWithSpacing();
        

        yield return new WaitForSeconds(Mathf.Max(0f, skillAnimationDuration - skillCastDelay));

        skillTimer = skillCooldown;
        isCasting = false;
        PlayState(moveStateName);
    }

    private IEnumerator SummonRoutine()
    {
        isCasting = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Summon thì boss nhìn về player
        
        ForceFacePlayer();
        PlayState(summonStateName);
        yield return new WaitForSeconds(summonCastDelay);

        SpawnMiniReapers();

        yield return new WaitForSeconds(Mathf.Max(0f, summonAnimationDuration - summonCastDelay));

        summonTimer = summonCooldown;
        isCasting = false;
        PlayState(moveStateName);
    }

    private IEnumerator DeathRoutine()
    {
        defeated = true;
        isCasting = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        PlayState(deathStateName);
        yield return new WaitForSeconds(deathAnimationDuration);

        SpawnDeathRewards();
        Destroy(gameObject);
    }

    private void SpawnSlashWave()
    {
        if (slashWavePrefab == null || PlayerController.Instance == null)
        {
            return;
        }

        Vector3 spawnPosition = slashSpawnPoint != null
            ? slashSpawnPoint.position
            : transform.position + new Vector3(spriteRenderer != null && spriteRenderer.flipX ? -1.2f : 1.2f, 1.2f, 0f);

        Vector2 direction = ((Vector2)PlayerController.Instance.transform.position - (Vector2)spawnPosition).normalized;
        ReaperSlashWave slashWave = Instantiate(slashWavePrefab, spawnPosition, Quaternion.identity);
        slashWave.Initialize(direction, slashWaveSpeed, slashWaveDamage, slashWaveLifetime);
    }

    private void SpawnMiniReapers()
    {
        if (reaperMiniPrefab == null)
        {
            return;
        }

        int remainingSlots = Mathf.Max(0, maxActiveSummons - activeSummons.Count);
        if (remainingSlots == 0)
        {
            return;
        }

        int summonsToCreate = Mathf.Min(2, remainingSlots);
        Vector3 basePosition = summonPoint != null ? summonPoint.position : transform.position;

        for (int i = 0; i < summonsToCreate; i++)
        {
            float horizontalOffset = summonsToCreate == 1
                ? 0f
                : (i == 0 ? -summonSpacing : summonSpacing);

            Vector3 spawnPosition = basePosition + new Vector3(horizontalOffset, -0.2f, 0f);
            ReaperMiniBoss mini = Instantiate(reaperMiniPrefab, spawnPosition, Quaternion.identity);
            activeSummons.Add(mini);
        }
    }

    private bool CanSummon()
    {
        return reaperMiniPrefab != null && summonTimer <= 0f && activeSummons.Count < maxActiveSummons;
    }

    private bool CanUseSkill(float distanceToPlayer)
    {
        return !string.IsNullOrWhiteSpace(skillStateName)
            && skillTimer <= 0f
            && distanceToPlayer > skillMinimumDistanceFromPlayer;
    }

    private void CleanupSummons()
    {
        activeSummons.RemoveAll(mini => mini == null);
    }

    private void TeleportTowardPlayerWithSpacing()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        Vector2 playerPosition = PlayerController.Instance.transform.position;
        Vector2 currentPosition = transform.position;
        Vector2 toPlayer = playerPosition - currentPosition;
        float currentDistance = toPlayer.magnitude;

        if (currentDistance <= skillMinimumDistanceFromPlayer)
        {
            return;
        }

        Vector2 directionToPlayer = currentDistance > 0.001f ? toPlayer / currentDistance : Vector2.right;
        float targetDistance = Mathf.Max(skillMinimumDistanceFromPlayer, skillPreferredDistanceFromPlayer);
        float maxAllowedDistance = Mathf.Min(targetDistance, currentDistance - 0.1f);
        float distanceAfterTeleport = Mathf.Max(skillMinimumDistanceFromPlayer, maxAllowedDistance);

        Vector2 targetPosition = playerPosition - directionToPlayer * distanceAfterTeleport;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

        // Sau khi teleport vẫn giữ mặt nhìn vào player
        
    }


    private void PlayOptionalSkillState()
    {
        if (animator != null && !string.IsNullOrWhiteSpace(skillStateName))
        {
            // Optional secondary state if you want to swap the attack visual later.
        }
    }

    private void PlayState(string stateName)
    {
        if (animator != null
            && !string.IsNullOrWhiteSpace(stateName)
            && !animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            animator.Play(stateName, 0, 0f);
        }
    }
    private void ForceFacePlayer()
    {
        if (spriteRenderer == null || PlayerController.Instance == null) return;

        bool isPlayerRight = PlayerController.Instance.transform.position.x > transform.position.x;
        spriteRenderer.flipX = isFacingRightDefault ? !isPlayerRight : isPlayerRight;
    }
}