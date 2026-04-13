using UnityEngine;
using System.Collections.Generic;

public class ProjectileWeaponPrefab : MonoBehaviour
{
    public ProjectileWeapon weapon;
    private Vector2 targetPosition;
    private Rigidbody2D rb;
    private float timer;
    public int projectileIndex; // Set by ProjectileWeapon when spawning
    [SerializeField] private float spreadAngle = 15f; // Total spread angle for all projectiles
    
    private int piercesRemaining; // How many more enemies this projectile can pierce
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>(); // Track already hit enemies

    void Start()
    {
        weapon = GameObject.Find("ProjectileWeapon").GetComponent<ProjectileWeapon>();
        rb = GetComponent<Rigidbody2D>();
        timer = weapon.stats[weapon.weaponLevel].duration;
        piercesRemaining = weapon.stats[weapon.weaponLevel].pierce;
        
        // Set initial direction toward mouse cursor with spread
        SetDirectionToMouse();
    }

    void Update()
    {
        // Handle lifetime
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // Move in the set direction (no homing, straight line)
        // Direction was set once at spawn and projectile continues in that direction
    }

    private void SetDirectionToMouse()
    {
        // Get mouse position in world space
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Calculate base direction from projectile spawn point to mouse
        Vector2 baseDirection = (mousePosition - (Vector2)transform.position).normalized;
        
        // Get total projectile count from weapon stats
        int totalProjectiles = weapon.stats[weapon.weaponLevel].count;
        
        // Calculate spread offset for this specific projectile
        float angleOffset = 0f;
        if (totalProjectiles > 1)
        {
            // Calculate spread per projectile
            float totalSpread = spreadAngle;
            float startAngle = -totalSpread / 2f;
            float angleStep = totalSpread / (totalProjectiles - 1);
            
            angleOffset = startAngle + (angleStep * projectileIndex);
        }
        
        // Apply angle offset to direction
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float finalAngle = baseAngle + angleOffset;
        float finalAngleRad = finalAngle * Mathf.Deg2Rad;
        
        Vector2 direction = new Vector2(Mathf.Cos(finalAngleRad), Mathf.Sin(finalAngleRad));
        
        // Set velocity once
        rb.linearVelocity = direction * weapon.stats[weapon.weaponLevel].speed;
        
        // Rotate projectile to face movement direction
        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemies"))
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                // Mark this enemy as hit
                hitEnemies.Add(enemy);
                
                // Deal damage
                enemy.TakeDamage(weapon.stats[weapon.weaponLevel].damage);
                
                // Reduce pierce count
                piercesRemaining--;
                
                // Destroy projectile if no pierces remaining
                if (piercesRemaining < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
