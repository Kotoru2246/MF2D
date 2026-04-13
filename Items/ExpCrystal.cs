using UnityEngine;

public class ExpCrystal : Item
{
    [SerializeField] private int XPValue;

    public void SetXPValue(int value)
    {
        XPValue = value;
    }

    protected override void AwardAndDestroy()
    {
        if (collected) return;
        collected = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        PlayerController.Instance.GetExperience(XPValue);
        Destroy(gameObject);
    }
}
