using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        [Tooltip("Minute this enemy starts appearing")]
        public float unlockTimeMinutes;
        [Tooltip("Minute this enemy stops spawning (0 = never stops)")]
        public float stopTimeMinutes = 0f;
        [Tooltip("Spawn weight — higher = more frequent")]
        public float weight = 1f;
    }

    [System.Serializable]
    public class SpecialEvent
    {
        public string eventName;
        [Tooltip("Minute this event triggers")]
        public float triggerTimeMinutes;
        public EventType eventType;
        public GameObject bossPrefab;
        [Tooltip("Duration of timed events like swarms/gold fever (seconds)")]
        public float duration = 15f;
        [Tooltip("How many enemies to spawn during swarm events")]
        public int swarmCount = 30;
        [HideInInspector] public bool triggered;
    }

    public enum EventType
    {
        SwarmRush,      // Massive wave of weak enemies
        EliteSpawn,     // Spawn a few buffed enemies
        GoldFever,      // Enemies drop coins at 100% rate for a duration
        MiniBoss,       // Spawn a mini boss
        FinalBoss       // Spawn the final boss
    }

    [Header("Run Settings")]
    [SerializeField] private float runDurationMinutes = 30f;

    [Header("Enemy Pool")]
    [SerializeField] private List<EnemyEntry> enemyPool;

    [Header("Special Events")]
    [SerializeField] private List<SpecialEvent> specialEvents;

    [Header("Spawn Timing")]
    [Tooltip("Base seconds between spawns at minute 0")]
    [SerializeField] private float baseSpawnInterval = 1.5f;
    [Tooltip("Minimum seconds between spawns at minute 30")]
    [SerializeField] private float minSpawnInterval = 0.2f;
    [Tooltip("How many enemies per spawn at minute 0")]
    [SerializeField] private int baseSpawnCount = 1;
    [Tooltip("How many enemies per spawn at minute 30")]
    [SerializeField] private int maxSpawnCount = 5;

    [Header("Stat Scaling Over Time")]
    [SerializeField] private float maxHealthMultiplier = 5f;
    [SerializeField] private float maxDamageMultiplier = 3f;
    [SerializeField] private float maxSpeedMultiplier = 1.5f;
    [SerializeField] private float maxExpMultiplier = 3f;

    [Header("Spawn Area")]
    public Transform minPos;
    public Transform maxPos;

    [Header("Spawn Validation")]
    [SerializeField] private string boundaryTag = "Boundary";
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private float repositionStep = 0.5f;

    [Header("Gold Fever")]
    [SerializeField] private float goldFeverCoinChance = 1f;

    private float spawnTimer;
    private bool runComplete;
    private bool goldFeverActive;

    public bool IsGoldFeverActive => goldFeverActive;

    private void Start()
    {
        runComplete = false;
        goldFeverActive = false;

        if (UIController.Instance != null)
            UIController.Instance.UpdateWaveNumber(0);
    }

    void Update()
    {
        if (runComplete) return;
        if (PlayerController.Instance == null || !PlayerController.Instance.gameObject.activeSelf) return;

        float gameTime = GameManager.Instance.gameTime;
        float runDurationSeconds = runDurationMinutes * 60f;

        // Check if run is complete
        if (gameTime >= runDurationSeconds)
        {
            runComplete = true;
            // Optionally trigger victory here
            return;
        }

        // Check special events
        CheckSpecialEvents(gameTime);

        // Regular spawning
        float progress = Mathf.Clamp01(gameTime / runDurationSeconds);
        float currentInterval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, progress);

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            int spawnCount = Mathf.RoundToInt(Mathf.Lerp(baseSpawnCount, maxSpawnCount, progress));
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy(gameTime, progress);
            }
        }
    }

    private void SpawnEnemy(float gameTime, float progress)
    {
        // Build pool of available enemies based on current time
        float currentMinute = gameTime / 60f;
        List<EnemyEntry> available = new List<EnemyEntry>();
        float totalWeight = 0f;

        foreach (var entry in enemyPool)
        {
            if (currentMinute >= entry.unlockTimeMinutes &&
                (entry.stopTimeMinutes <= 0f || currentMinute < entry.stopTimeMinutes))
            {
                available.Add(entry);
                totalWeight += entry.weight;
            }
        }

        if (available.Count == 0) return;

        // Weighted random selection
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        EnemyEntry selected = available[0];

        foreach (var entry in available)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
            {
                selected = entry;
                break;
            }
        }

        Vector2 spawnPoint = GetValidSpawnPoint();
        GameObject spawned = Instantiate(selected.prefab, spawnPoint, Quaternion.identity);

        // Scale stats based on time progress
        Enemy enemy = spawned.GetComponent<Enemy>();
        if (enemy != null)
        {
            float healthMul = Mathf.Lerp(1f, maxHealthMultiplier, progress);
            float damageMul = Mathf.Lerp(1f, maxDamageMultiplier, progress);
            float speedMul = Mathf.Lerp(1f, maxSpeedMultiplier, progress);
            float expMul = Mathf.Lerp(1f, maxExpMultiplier, progress);

            enemy.ScaleStats(healthMul, damageMul, speedMul, expMul);
        }
    }

    // ─── Special Events ────────────────────────────────────
    private void CheckSpecialEvents(float gameTime)
    {
        float currentMinute = gameTime / 60f;

        foreach (var evt in specialEvents)
        {
            if (!evt.triggered && currentMinute >= evt.triggerTimeMinutes)
            {
                evt.triggered = true;
                TriggerEvent(evt, gameTime);
            }
        }
    }

    private void TriggerEvent(SpecialEvent evt, float gameTime)
    {
        float progress = Mathf.Clamp01(gameTime / (runDurationMinutes * 60f));

        switch (evt.eventType)
        {
            case EventType.SwarmRush:
                StartCoroutine(SwarmRushRoutine(evt, progress));
                break;

            case EventType.EliteSpawn:
                SpawnElites(evt, progress);
                break;

            case EventType.GoldFever:
                StartCoroutine(GoldFeverRoutine(evt));
                break;

            case EventType.MiniBoss:
            case EventType.FinalBoss:
                SpawnBoss(evt, progress);
                break;
        }

        Debug.Log($"<color=yellow>Event triggered: {evt.eventName} at {evt.triggerTimeMinutes} min</color>");
    }

    private IEnumerator SwarmRushRoutine(SpecialEvent evt, float progress)
    {
        // Rapidly spawn a burst of enemies
        float interval = evt.duration / evt.swarmCount;

        for (int i = 0; i < evt.swarmCount; i++)
        {
            if (PlayerController.Instance == null || !PlayerController.Instance.gameObject.activeSelf)
                yield break;

            SpawnEnemy(GameManager.Instance.gameTime, progress);
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnElites(SpecialEvent evt, float progress)
    {
        // Spawn 3-5 heavily buffed enemies
        int eliteCount = Random.Range(3, 6);
        for (int i = 0; i < eliteCount; i++)
        {
            Vector2 spawnPoint = GetValidSpawnPoint();

            // Pick a random enemy from the pool
            if (enemyPool.Count == 0) return;
            GameObject prefab = enemyPool[Random.Range(0, enemyPool.Count)].prefab;

            GameObject spawned = Instantiate(prefab, spawnPoint, Quaternion.identity);
            Enemy enemy = spawned.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Elites get 3x the current scaling
                float healthMul = Mathf.Lerp(1f, maxHealthMultiplier, progress) * 3f;
                float damageMul = Mathf.Lerp(1f, maxDamageMultiplier, progress) * 2f;
                float speedMul = Mathf.Lerp(1f, maxSpeedMultiplier, progress) * 0.8f; // Slower but tankier
                float expMul = Mathf.Lerp(1f, maxExpMultiplier, progress) * 5f;

                enemy.ScaleStats(healthMul, damageMul, speedMul, expMul);

                // Visually indicate elite (optional — scale up slightly)
                spawned.transform.localScale *= 1.5f;
            }
        }
    }

    private IEnumerator GoldFeverRoutine(SpecialEvent evt)
    {
        goldFeverActive = true;
        // Optionally show UI notification
        if (UIController.Instance != null)
            UIController.Instance.ShowWaveBonusNotification(0); // Reuse or create a gold fever notification

        yield return new WaitForSeconds(evt.duration);
        goldFeverActive = false;
    }

    private void SpawnBoss(SpecialEvent evt, float progress)
    {
        if (evt.bossPrefab == null) return;

        Vector2 spawnPoint = GetValidSpawnPoint();
        GameObject boss = Instantiate(evt.bossPrefab, spawnPoint, Quaternion.identity);

        Enemy bossEnemy = boss.GetComponent<Enemy>();
        if (bossEnemy != null)
        {
            float bossMultiplier = evt.eventType == EventType.FinalBoss ? 2f : 1f;
            float healthMul = Mathf.Lerp(1f, maxHealthMultiplier, progress) * 10f * bossMultiplier;
            float damageMul = Mathf.Lerp(1f, maxDamageMultiplier, progress) * 3f * bossMultiplier;
            float speedMul = 1f;
            float expMul = Mathf.Lerp(1f, maxExpMultiplier, progress) * 10f * bossMultiplier;

            bossEnemy.ScaleStats(healthMul, damageMul, speedMul, expMul);
        }
    }

    // ─── Spawn Position Helpers ────────────────────────────
    private Vector2 GetValidSpawnPoint()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 spawnPoint = RandomSpawnPoint();
            if (!IsOnBoundary(spawnPoint))
                return spawnPoint;

            Vector2 validPoint = FindNearestValidPoint(spawnPoint);
            if (!IsOnBoundary(validPoint))
                return validPoint;
        }

        return RandomSpawnPoint();
    }

    private bool IsOnBoundary(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(boundaryTag))
                return true;
        }
        return false;
    }

    private Vector2 FindNearestValidPoint(Vector2 originalPoint)
    {
        Vector2 center = new Vector2(
            (minPos.position.x + maxPos.position.x) / 2f,
            (minPos.position.y + maxPos.position.y) / 2f
        );
        Vector2 directionToCenter = (center - originalPoint).normalized;

        Vector2 testPoint = originalPoint;
        for (int i = 0; i < 20; i++)
        {
            testPoint += directionToCenter * repositionStep;
            if (!IsOnBoundary(testPoint))
                return testPoint;
        }

        return originalPoint;
    }

    private Vector2 RandomSpawnPoint()
    {
        Vector2 spawnPoint;
        if (Random.Range(0f, 1f) > 0.5f)
        {
            spawnPoint.x = Random.Range(minPos.position.x, maxPos.position.x);
            spawnPoint.y = Random.Range(0f, 1f) > 0.5f ? minPos.position.y : maxPos.position.y;
        }
        else
        {
            spawnPoint.y = Random.Range(minPos.position.y, maxPos.position.y);
            spawnPoint.x = Random.Range(0f, 1f) > 0.5f ? minPos.position.x : maxPos.position.x;
        }
        return spawnPoint;
    }
}