using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkyBeamWeaponPrefab : MonoBehaviour
{
    public SkyBeamWeapon weapon;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    // How many seconds into the animation damage is dealt (tune to match the bright-beam frame)
    [SerializeField] private float damageDelay = 0.25f;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private float impactEffectDuration = 0.5f;
    [SerializeField] private float effectScaleMultiplier = 2f;

    private float damage;
    private float impactRadius;
    private float animationDuration;

    void Start()
    {
        weapon = GameObject.Find("SkyBeamWeapon")?.GetComponent<SkyBeamWeapon>();

        if (weapon != null)
        {
            damage = weapon.stats[weapon.weaponLevel].damage;
            impactRadius = weapon.stats[weapon.weaponLevel].range;
            animationDuration = weapon.stats[weapon.weaponLevel].duration;
        }
        else
        {
            damage = 10f;
            impactRadius = 1.5f;
            animationDuration = 0.9f;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        // Read clip length directly from the Animator so destroy timing always matches the animation
        float clipLength = 0f;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.length > clipLength)
                    clipLength = clip.length;
            }
        }
        animationDuration = clipLength > 0f ? clipLength : animationDuration;

        StartCoroutine(BeamRoutine());
    }

    private IEnumerator BeamRoutine()
    {
        // Wait for the bright-beam frame before dealing damage
        yield return new WaitForSeconds(damageDelay);
        Impact();

        // Wait for the rest of the animation to finish, then destroy
        yield return new WaitForSeconds(animationDuration - damageDelay);
        Destroy(gameObject);
    }

    private void Impact()
    {
        // Spawn impact effect
        if (impactEffect != null)
        {
            float effectScale = impactRadius * effectScaleMultiplier;
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * effectScale;
            Destroy(effect, impactEffectDuration);
        }

        // Deal AOE damage to all enemies in impact radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);
        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemies"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && !damagedEnemies.Contains(enemy))
                {
                    damagedEnemies.Add(enemy);
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}
