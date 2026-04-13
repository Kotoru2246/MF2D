using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class ItemLevelSaveData
{
    public string itemID;
    public int level;

    public ItemLevelSaveData(string id, int lvl)
    {
        itemID=id;
        level = lvl;
    }
}
[System.Serializable]
public class PlayerSaveData
{
    public int currentGold;
    public List<ItemLevelSaveData> savedUpgrades = new List<ItemLevelSaveData>();
}
