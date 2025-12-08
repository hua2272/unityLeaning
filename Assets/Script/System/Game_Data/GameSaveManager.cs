using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager instance;

    private GameDataManager gameDataManager;
    private Player player;
    public WeaponSlotsManager weaponSlotsManager;
    private GameData currentGameData = new GameData();
    
    // 存储被摧毁的地形对象
    private Dictionary<string, DestructibleTileData> destroyedTiles = new Dictionary<string, DestructibleTileData>();
    
    private void Awake()
    {
        Debug.Log("<color=#FF0000>-------GameSaveManager instance-------</color>");
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
        
        // 保存地形数据
        currentGameData.destroyedTiles = GetAllDestroyedTiles();
        
        string jsonData = JsonUtility.ToJson(currentGameData, prettyPrint: true);
        
        string savePath = GetSavePath(0);
        try
        {
            File.WriteAllText(savePath, jsonData);
            Debug.Log("游戏保存成功: " + savePath);
            Debug.Log($"保存了 {currentGameData.destroyedTiles?.Count ?? 0} 个被摧毁的地形对象");
        }
        catch (Exception e)
        {
            Debug.LogError("保存游戏失败: " + e.Message);
        }
    }
    
    // 地形保存相关方法
    public void SaveTileState(DestructibleTileData tileData)
    {
        tileData.sceneName = SceneManager.GetActiveScene().name;
        destroyedTiles[tileData.tileId] = tileData;
    }
    
    public bool IsTileDestroyed(string tileId)
    {
        return destroyedTiles.ContainsKey(tileId) && destroyedTiles[tileId].isDestroyed;
    }
    
    public List<DestructibleTileData> GetAllDestroyedTiles()
    {
        return new List<DestructibleTileData>(destroyedTiles.Values);
    }
    
    public void LoadTileStates(List<DestructibleTileData> tileDataList)
    {
        destroyedTiles.Clear();
        foreach (var tileData in tileDataList)
        {
            destroyedTiles[tileData.tileId] = tileData;
        }
    }
    
    public void ClearAllTileStates()
    {
        destroyedTiles.Clear();
    }
    
    public void RemoveTileState(string tileId)
    {
        if (destroyedTiles.ContainsKey(tileId))
        {
            destroyedTiles.Remove(tileId);
        }
    }
    
    public static bool DoesSaveExist(int slotId)
    {
        return File.Exists(GetSavePath(slotId));//todo 是否有逻辑问题，查询文件目录却新建了文件目录
    }
    
    public static string GetSavePath(int slotId)
    {
        string gameDirectory = Path.GetDirectoryName(Application.dataPath);
        if (Application.isEditor)
        {
            gameDirectory = Application.persistentDataPath;
        }
        string saveDirectory = Path.Combine(gameDirectory, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        return Path.Combine(saveDirectory, "gameSave.dat");
    }
}