using UnityEngine;

public class GetCoin : Weapon
{
    public override void LevelUp()
    {
        // Fixed amount every time — always uses stats[0]
        if (ShopManager.Instance != null)
            ShopManager.Instance.AddGold((int)stats[0].damage);
    }
}
