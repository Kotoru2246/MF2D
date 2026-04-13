using UnityEngine;

public class HealthItem : Item
{
    [SerializeField] [Range(0f, 1f)] private float healPercent = 0.3f;

    public void SetHealPercent(float percent)
    {
        healPercent = percent;
    }

    protected override void AwardAndDestroy()
    {
        if (collected) return;
        collected = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (PlayerController.Instance != null)
        {
            float healAmount = PlayerController.Instance.playerMaxHealth * healPercent;
            PlayerController.Instance.playerCurrentHealth = Mathf.Min(
                PlayerController.Instance.playerCurrentHealth + healAmount,
                PlayerController.Instance.playerMaxHealth
            );
            UIController.Instance.UpdatePlayerHealthSlider();
        }

        Destroy(gameObject);
    }
}
