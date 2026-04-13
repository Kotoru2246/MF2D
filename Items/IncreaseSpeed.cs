using UnityEngine;

public class IncreaseSpeed : Weapon
{
    void Start()
    {
        PlayerController.Instance.levelUpSpeedBonus += stats[weaponLevel].damage;
        PlayerController.Instance.UpdateStats();
    }

    public override void LevelUp()
    {
        base.LevelUp();
        PlayerController.Instance.levelUpSpeedBonus += stats[weaponLevel].damage;
        PlayerController.Instance.UpdateStats();
    }
}
