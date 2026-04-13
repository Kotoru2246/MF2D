using System.Collections.Generic;
using UnityEngine;

public class SpiningWeaponPrefabs : MonoBehaviour
{
    public List<Enemy> enemiesInRange;
    public SpiningWeapon weapon;
    private float counter;
    private float timer;
    private Vector3 targetSize;

    private enum SwordState { FlyingOut, Orbiting, Returning }
    private SwordState state = SwordState.FlyingOut;

    [SerializeField] private float flySpeed = 5f;

    void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    void Start()
    {
        weapon = GetComponentInParent<SpiningWeapon>();

        if (weapon == null)
        {
            Debug.LogError("SpiningWeaponPrefabs: Could not find SpiningWeapon in parent!");
            return;
        }

        timer = weapon.stats[weapon.weaponLevel].duration;
        targetSize = Vector3.one * weapon.stats[weapon.weaponLevel].range;
        state = SwordState.FlyingOut;
    }

    void Update()
    {
        if (weapon == null) return;

        if (state == SwordState.FlyingOut)
        {
            // Grow from zero to full size — sword "flies out"
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetSize, Time.deltaTime * flySpeed);
            if (transform.localScale == targetSize)
            {
                state = SwordState.Orbiting;
            }
            return;
        }

        if (state == SwordState.Orbiting)
        {
            // Deal periodic damage while orbiting
            counter -= Time.deltaTime;
            if (counter <= 0)
            {
                counter = weapon.stats[weapon.weaponLevel].speed;
                for (int i = 0; i < enemiesInRange.Count; i++)
                {
                    if (enemiesInRange[i] != null)
                        enemiesInRange[i].TakeDamage(weapon.stats[weapon.weaponLevel].damage);
                }
            }

            // Count down duration
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                state = SwordState.Returning;
            }
            return;
        }

        if (state == SwordState.Returning)
        {
            // Shrink back to zero — sword "returns to player"
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * flySpeed);
            // Also move local position back toward center
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Time.deltaTime * flySpeed);

            if (transform.localScale.magnitude <= 0.05f)
            {
                weapon.OnOrbiterFinished();
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemies"))
            enemiesInRange.Add(collider.GetComponent<Enemy>());
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemies"))
            enemiesInRange.Remove(collider.GetComponent<Enemy>());
    }
}


