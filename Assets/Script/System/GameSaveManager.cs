using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class GameSaveManager : MonoBehaviour
{
    [System.Serializable]
    public class GameData
    {
        public int playerLevel;
        public float playerHealth;
        public string currentWeapon;
        public Vector3 playerPosition;
        public string[] inventoryItems;
    }
    
    public Player player;
    private GameData currentGameData = new GameData(); // 当前游戏数据
    
    public void SaveGame()
    {
        currentGameData.playerLevel = 5;
        currentGameData.playerHealth = 85.5f;
        // currentGameData.currentWeapon = playerEquipment.GetCurrentWeapon().weaponName;
        currentGameData.playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, 0f);
        currentGameData.inventoryItems = new string[] { "Sword", "Potion", "Key" };
        string jsonData = JsonUtility.ToJson(currentGameData, prettyPrint: true);
        
        string savePath = GetSavePath();
        try
        {
            File.WriteAllText(savePath, jsonData);
            Debug.Log("游戏保存成功: " + savePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("保存游戏失败: " + e.Message);
        }
    }
    
    
    public static bool DoesSaveExist()
    {
        //return PlayerPrefs.HasKey("GameSaveData");
        //todo 保存路径优化（去除空格等因素）
        return File.Exists(GetSavePath());
    }
    
    private static string GetSavePath()
    {
        // 获取游戏可执行文件所在目录
        string gameDirectory = Path.GetDirectoryName(Application.dataPath);
        // 如果是在编辑器中运行，路径会有所不同
        if (Application.isEditor)
        {
            gameDirectory = Application.persistentDataPath;
        }
        // 创建保存目录（如果不存在）
        string saveDirectory = Path.Combine(gameDirectory, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        // 返回完整的保存文件路径
        return Path.Combine(saveDirectory, "gameSave.dat");
    }
}