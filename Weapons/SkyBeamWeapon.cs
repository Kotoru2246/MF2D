using UnityEngine;

public class SkyBeamWeapon : Weapon
{
    [SerializeField] private GameObject prefab;
    private float spawnCounter;

    void Update()
    {
        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0)
        {
            spawnCounter = GetCooldown();

            int beamCount = stats[weaponLevel].count;

            // Gather all enemies in range to target
            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, stats[weaponLevel].range);

            for (int i = 0; i < beamCount; i++)
            {
                Vector3 targetPosition;

                // Try to pick a random enemy within range, otherwise pick a random position
                System.Collections.Generic.List<Collider2D> validTargets = new System.Collections.Generic.List<Collider2D>();
                foreach (Collider2D col in enemiesInRange)
                {
                    if (col.CompareTag("Enemies"))
                        validTargets.Add(col);
                }

                if (validTargets.Count > 0)
                {
                    int randomIndex = Random.Range(0, validTargets.Count);
                    targetPosition = validTargets[randomIndex].transform.position;
                }
                else
                {
                    Vector2 randomOffset = Random.insideUnitCircle * stats[weaponLevel].range;
                    targetPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
                }

                Instantiate(prefab, targetPosition, Quaternion.identity);
            }
        }
    }
}
