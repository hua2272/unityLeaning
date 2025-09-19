using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    [Serializable] public class GameData
    {
        public int playerLevel;
        public float playerHealth;
        public int equippedSlotId;
        public string scene;
        public Vector3 playerPosition;
        public string[] inventoryItems;
    }
    
    public Player player;
    public WeaponSlotsManager weaponSlotsManager;
    public WeaponSlotUI weaponSlotUI;
    private GameData currentGameData = new GameData();
    
    public void SaveGame()
    {
        currentGameData.playerLevel = 5;
        currentGameData.playerHealth = 85.5f;
        currentGameData.playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, 0f);
        currentGameData.equippedSlotId = weaponSlotsManager.equippedSlotId;
        currentGameData.scene = SceneManager.GetActiveScene().name;
        currentGameData.inventoryItems = weaponSlotsManager.GetObtainedWeaponsName();
        string jsonData = JsonUtility.ToJson(currentGameData, prettyPrint: true);
        
        string savePath = GetSavePath();
        try
        {
            File.WriteAllText(savePath, jsonData);
            Debug.Log("游戏保存成功: " + savePath);
        }
        catch (Exception e)
        {
            Debug.LogError("保存游戏失败: " + e.Message);
        }
    }
    
    
    public static bool DoesSaveExist()
    {
        return File.Exists(GetSavePath());
    }
    
    public static string GetSavePath()
    {
        //todo 保存路径优化（去除空格等因素）
        string gameDirectory = Path.GetDirectoryName(Application.dataPath);     //获取游戏可执行文件所在目录
        if (Application.isEditor)                                               //如果是在编辑器中运行，路径会有所不同
        {
            gameDirectory = Application.persistentDataPath;
        }
        string saveDirectory = Path.Combine(gameDirectory, "Saves");            //创建保存目录（如果不存在
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        return Path.Combine(saveDirectory, "gameSave.dat");                     //返回完整的保存文件路径
    }
}