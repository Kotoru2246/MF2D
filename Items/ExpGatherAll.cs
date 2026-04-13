using UnityEngine;

public class ExpGatherAll : Item
{
    protected override void AwardAndDestroy()
    {
        if (collected) return;
        collected = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Pull every ExpCrystal in the scene toward the player
        ExpCrystal[] allCrystals = FindObjectsByType<ExpCrystal>(FindObjectsSortMode.None);
        foreach (ExpCrystal crystal in allCrystals)
        {
            crystal.OnPickedUp();
        }

        Destroy(gameObject);
    }
}
