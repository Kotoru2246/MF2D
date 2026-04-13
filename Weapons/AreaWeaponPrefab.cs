using UnityEngine;
using System.Collections.Generic;

public class AreaWeaponPrefab : MonoBehaviour
{
    public AreaWeapon weapon;
    private Vector3 targetSize;
    private float timer;
    public List<Enemy> enemiesInRange = new List<Enemy>();
    private float counter;
    
    void Start()
    {
        weapon = GameObject.Find("AreaWeapon").GetComponent<AreaWeapon>();
        //Destroy(gameObject, weapon.duration);
        targetSize = Vector3.one * weapon.stats[weapon.weaponLevel].range;
        transform.localScale = Vector3.zero;
        timer = weapon.stats[weapon.weaponLevel].duration;
    }

    // Update is called once per frame
    void Update()
    {
        //grow and shrink towards targetSize
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetSize, Time.deltaTime * 2);
        //shrink and only then destroy
        timer -= Time.deltaTime;
        if (timer <= 0) { 
            targetSize = Vector3.zero;
            if (transform.localScale.x == 0)
            {
                Destroy(gameObject);
            }
        }
        // periodic damage
        counter -= Time.deltaTime;
        if (counter <= 0)
        {
            counter = weapon.stats[weapon.weaponLevel].speed;
            
            // Remove null enemies (destroyed by other weapons) before dealing damage
            enemiesInRange.RemoveAll(enemy => enemy == null);
            
            for (int i = 0; i < enemiesInRange.Count; i++)
            {
                enemiesInRange[i].TakeDamage(weapon.stats[weapon.weaponLevel].damage);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemies")) { 
            enemiesInRange.Add(collider.GetComponent<Enemy>());
        }
    }
    
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemies"))
        {
            enemiesInRange.Remove(collider.GetComponent<Enemy>());
        }
    }
}
