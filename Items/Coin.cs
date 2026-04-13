using UnityEngine;

public class Coin : Item
{
    [SerializeField] private int coinValue = 1;

    public void SetCoinValue(int value)
    {
        coinValue = value;
    }

    protected override void AwardAndDestroy()
    {
        if (collected) return;
        collected = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (ShopManager.Instance != null)
            ShopManager.Instance.AddGold(coinValue);

        Destroy(gameObject);
    }
}
