using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    [Serializable] public class GameData
    {
        public int playerLevel;
        public int playerHealth;
        public int equippedSlotId;
        public string scene;
        public Vector3 playerPosition;
        public string[] inventoryItems;
        public SerializableDictionary unlockedSkills;       //JsonUtility无法解析字典类型
    }
    
    [Serializable] public class SerializableDictionary
    {
        [Serializable] public class KeyValuePair
        {
            public string key;
            public int value;
        }
        public List<KeyValuePair> items = new List<KeyValuePair>();
    }
    
    public Player player;
    public WeaponSlotsManager weaponSlotsManager;
    private SkillTreeManager skillTreeManager;
    private PlayerStatus playerStatus;
    private GameData currentGameData = new GameData();

    private void Awake()
    {
        playerStatus = PlayerManager.instance.playerStatus;
        skillTreeManager = SkillTreeManager.instance;
    }

    public void SaveGame()
    {
        currentGameData.unlockedSkills = new SerializableDictionary();
        foreach (var kvp in skillTreeManager.unlockedSkills)
        {
            currentGameData.unlockedSkills.items.Add(new SerializableDictionary.KeyValuePair 
            { 
                key = kvp.Key, 
                value = kvp.Value 
            });
        }
        currentGameData.playerLevel = 5;
        currentGameData.playerHealth = playerStatus.health.getValue();
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