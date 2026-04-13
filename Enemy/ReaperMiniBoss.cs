using System.Collections;
using UnityEngine;

public class ReaperMiniBoss : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private string moveStateName = "ReaperMiniIdle";
    [SerializeField] private string deathStateName = "ReaperMiniDie";
    [SerializeField] private float deathAnimationDuration = 0.85f;

    private bool defeated;

    protected override void Awake()
    {
        base.Awake();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    protected override void Start()
    {
        base.Start();
        PlayState(moveStateName);
    }

    protected override void FixedUpdate()
    {
        if (defeated)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            return;
        }

        base.FixedUpdate();
        PlayState(moveStateName);
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
            DamageNumberController.Instance.CreateNumber(damageAmount, transform.position + Vector3.up * 1.2f);
        }

        pushCounter = pushTime;
        if (health <= 0f)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {
        defeated = true;

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

    private void PlayState(string stateName)
    {
        if (animator != null
            && !string.IsNullOrWhiteSpace(stateName)
            && !animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            animator.Play(stateName, 0, 0f);
        }
    }
}
