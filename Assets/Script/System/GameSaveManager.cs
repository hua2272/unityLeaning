using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager instance;

    private GameDataManager gameDataManager;
    private Player player;
    public WeaponSlotsManager weaponSlotsManager;
    private GameData currentGameData = new GameData();
    
    private void Awake()
    {
        Debug.Log("-------GameSaveManager instance-------");
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Start()
    {
        player = PlayerManager.instance.player;
        gameDataManager = GameDataManager.instance;
    }

    public void SaveGame()
    {
        currentGameData.unlockedSkills = gameDataManager.unlockedSkills;
        currentGameData.playerLevel = 5;
        currentGameData.playerHealth = player.playerStatus.health.getValue();
        currentGameData.playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, 0f);
        currentGameData.equippedSlotId = gameDataManager.equippedSlotId;
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