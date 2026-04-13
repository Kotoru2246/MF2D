using UnityEngine;

public class DropWeapon : Weapon
{
    [SerializeField] private GameObject prefab;
    private float spawnCounter;

    void Update()
    {
        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0)
        {
            spawnCounter = GetCooldown();
            
            int mineCount = stats[weaponLevel].count;
            
            for (int i = 0; i < mineCount; i++)
            {
                // Spawn mines around the player with random offset
                Vector2 randomOffset = Random.insideUnitCircle * stats[weaponLevel].range;
                Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
                
                Instantiate(prefab, spawnPosition, Quaternion.identity);
            }
        }
    }
}
