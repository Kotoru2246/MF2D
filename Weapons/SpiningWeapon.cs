using UnityEngine;

public class SpiningWeapon : Weapon
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float orbitRadius = 1.5f;
    private float currentAngle;
    private int activeOrbiterCount;

    void OnEnable()
    {
        currentAngle = 0f;
        activeOrbiterCount = 0;
        CancelInvoke();

        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        SpawnOrbiters();
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Update()
    {
        transform.localPosition = Vector3.zero;
        currentAngle += stats[weaponLevel].rotationSpeed * 180f * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    public void OnOrbiterFinished()
    {
        activeOrbiterCount--;
        if (activeOrbiterCount <= 0)
        {
            activeOrbiterCount = 0;
            Invoke(nameof(SpawnOrbiters), GetCooldown());
        }
    }

    private void SpawnOrbiters()
    {
        int swordCount = stats[weaponLevel].count;
        if (swordCount <= 0) return;

        activeOrbiterCount = swordCount;
        float angleStep = 360f / swordCount;

        for (int i = 0; i < swordCount; i++)
        {
            float angleDeg = i * angleStep;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 localOffset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * orbitRadius;
            Quaternion outwardRotation = Quaternion.Euler(0f, 0f, angleDeg);

            GameObject spawned = Instantiate(prefab, transform);
            spawned.transform.localPosition = localOffset;
            spawned.transform.localRotation = outwardRotation;
        }
    }
}

