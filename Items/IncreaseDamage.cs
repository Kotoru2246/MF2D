using UnityEngine;

public class IncreaseDamage : Weapon
{
    void Start()
    {
        PlayerController.Instance.levelUpDamageBonus += stats[weaponLevel].damage;
        PlayerController.Instance.UpdateStats();
    }

    public override void LevelUp()
    {
        base.LevelUp();
        PlayerController.Instance.levelUpDamageBonus += stats[weaponLevel].damage;
        PlayerController.Instance.UpdateStats();
    }
}
