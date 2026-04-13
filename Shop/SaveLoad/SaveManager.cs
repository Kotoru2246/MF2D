using UnityEngine;
using System.IO;
public static class SaveManager
{
   
    private static string SavePath => Application.persistentDataPath + "/PlayerData.json";

    public static void SaveGame(PlayerSaveData data)
    {
       
        string json = JsonUtility.ToJson(data, true);

      
        File.WriteAllText(SavePath, json);
        Debug.Log("<color=green>Đã lưu game thành công tại:</color> " + SavePath);
    }

    public static PlayerSaveData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            
            string json = File.ReadAllText(SavePath);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            Debug.Log("<color=cyan>Đã tải dữ liệu game!</color>");
            return data;
        }
        else
        {
           
            Debug.Log("<color=yellow>Không tìm thấy file Save, tạo dữ liệu mới!</color>");
            return new PlayerSaveData();
        }
    }
}
