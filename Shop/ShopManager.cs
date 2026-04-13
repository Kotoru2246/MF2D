using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; } 

    [Header("Player Economy")]
    public int currentGold = 1000;

    public List<ShopIteamdata> allshopiteams;

    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        LoadData();


    }
    
    private void LoadData()
    {
        PlayerSaveData data = SaveManager.LoadGame();
        currentGold = data.currentGold;
        upgradeLevels.Clear();
        foreach (var item in data.savedUpgrades)
        {
            upgradeLevels.Add(item.itemID, item.level);
        }

    }
    private void SaveData()
    {
        PlayerSaveData data = new PlayerSaveData();
        data.currentGold = currentGold;

       
        foreach (var kvp in upgradeLevels)
        {
            data.savedUpgrades.Add(new ItemLevelSaveData(kvp.Key, kvp.Value));
        }

        SaveManager.SaveGame(data);
    }
    public int GetItemLevel(string itemID)
    {
        if (upgradeLevels.TryGetValue(itemID, out int level))
        {
            return level;
        }
        return 0; 
    }
    public float GetTotalBonus(StatType statType)
    {
        float totalBonus = 0f;

        foreach (var item in allshopiteams)
        {
            if (item.statType == statType) 
            {
                int level = GetItemLevel(item.IteamID); 
                totalBonus += level * item.valueperLevel;
            }
        }

        return totalBonus;
    }
    public bool BuyUpgrade(ShopIteamdata item)
    {
        int currentLevel = GetItemLevel(item.IteamID);

        if (currentLevel >= item.maxLevel)
        {
            Debug.Log("Max Level!");
            return false;
        }

        int cost = item.GetCostForNextLevel(currentLevel);

        if (currentGold >= cost)
        {
          
            currentGold -= cost;

           
            upgradeLevels[item.IteamID] = currentLevel + 1;
            SaveData();
           
            return true;
        }
        else
        {
            Debug.Log("Not enough money!");
            return false;
        }
    }
    public void AddGold(int amount)
    {
        currentGold += amount;
        SaveData(); 
    }
}