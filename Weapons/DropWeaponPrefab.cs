using UnityEngine;
using System.Collections.Generic;

public class DropWeaponPrefab : MonoBehaviour
{
    public DropWeapon weapon;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionEffectDuration = 0.5f;
    [SerializeField] private float effectScaleMultiplier = 2f; // Adjust to match effect size to damage radius
    
    private float timer;
    private float damage;
    private float explosionRadius;
    private bool hasExploded = false;

    void Start()
    {
        weapon = GameObject.Find("DropWeapon")?.GetComponent<DropWeapon>();
        
        if (weapon != null)
        {
            timer = weapon.stats[weapon.weaponLevel].duration;
            damage = weapon.stats[weapon.weaponLevel].damage;
            explosionRadius = weapon.stats[weapon.weaponLevel].range;
        }
        else
        {
            timer = 10f;
            damage = 10f;
            explosionRadius = 1.5f;
        }
    }

    void Update()
    {
        // Self-destruct after duration expires (if no enemy triggered it)
        timer -= Time.deltaTime;
        if (timer <= 0 && !hasExploded)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasExploded) return;
        
        if (collider.CompareTag("Enemies"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // Spawn explosion effect, scale it to match explosion radius, and auto-destroy it
        if (explosionEffect != null)
        {
            // Scale effect to match damage radius
            float effectScale = explosionRadius * effectScaleMultiplier;
            
            // Offset Y position down by half the effect size (for bottom pivot)
            Vector3 effectPosition = transform.position;
            effectPosition.y -= effectScale / 2f;
            
            GameObject effect = Instantiate(explosionEffect, effectPosition, Quaternion.identity);
            effect.transform.localScale = Vector3.one * effectScale;
            Destroy(effect, explosionEffectDuration);
        }
        
        // Deal AOE damage to all enemies in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
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
        
        Destroy(gameObject);
    }

    // Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
