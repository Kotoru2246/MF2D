using UnityEngine;

public class Heal : Weapon
{
    public override void LevelUp()
    {
        // Fixed amount every time — always uses stats[0]
        float healAmount = PlayerController.Instance.playerMaxHealth * stats[0].damage;
        PlayerController.Instance.playerCurrentHealth = Mathf.Min(
            PlayerController.Instance.playerCurrentHealth + healAmount,
            PlayerController.Instance.playerMaxHealth
        );
        UIController.Instance.UpdatePlayerHealthSlider();
    }
}
