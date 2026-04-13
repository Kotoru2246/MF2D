using UnityEngine;

[CreateAssetMenu(fileName = "ShopIteamdata", menuName = "Scriptable Objects/ShopIteamdata")]
public class ShopIteamdata : ScriptableObject
{
    [Header("Thong tin co ban")]
    public string IteamID;  
    public string IteamName;
    public Sprite sprite;
    [TextArea]
    public string description;

    [Header("Chi so nang cap")]
    public StatType statType;
    public float valueperLevel;
    public int maxLevel;

    [Header("Kinh tế (Giá tiền)")]
    public int baseCost = 100;
    public float costMultiplier = 1.5f;
    public int GetCostForNextLevel(int currentLevel)
    {
        
        if (currentLevel >= maxLevel)
        {
            return -1;
        }
        float calculatedCost = baseCost * Mathf.Pow(costMultiplier, currentLevel);
        return Mathf.RoundToInt(calculatedCost);
    }

}
