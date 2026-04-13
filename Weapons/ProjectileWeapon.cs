using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [SerializeField] private GameObject prefab;
    private float spawnCounter;

    // Update is called once per frame
    void Update()
    {
        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0)
        {
            spawnCounter = GetCooldown();
            
            int projectileCount = stats[weaponLevel].count;
            
            for (int i = 0; i < projectileCount; i++)
            {
                GameObject projectile = Instantiate(prefab, transform.position, Quaternion.identity, transform);
                ProjectileWeaponPrefab prefabScript = projectile.GetComponent<ProjectileWeaponPrefab>();
                if (prefabScript != null)
                {
                    prefabScript.projectileIndex = i;
                }
            }
        }
    }
}
