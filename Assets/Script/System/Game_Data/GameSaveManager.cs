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

    public void SaveGame(int slotId)
    {
        currentGameData.unlockedSkills = gameDataManager.unlockedSkills;
        currentGameData.playerLevel = 5;
        currentGameData.playerHealth = player.playerStatus.health.getValue();
        currentGameData.playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, 0f);
        currentGameData.equippedSlotId = gameDataManager.equippedSlotId;
        currentGameData.scene = SceneManager.GetActiveScene().name;
        currentGameData.inventoryItems = weaponSlotsManager.GetObtainedWeaponsName();
        currentGameData.screenshot = "";
        
        currentGameData.destroyedTiles = GetAllDestroyedTiles();// 保存地形数据
        
        string jsonData = JsonUtility.ToJson(currentGameData, prettyPrint: true);
        string savePath = GetSavePath(slotId);
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
        string fileName = "gameSave" + slotId + ".dat";
        return Path.Combine(saveDirectory, fileName);
    }

    public static string GetParentSavePath()
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
        return saveDirectory;
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
    
    private string CaptureScreenshot(int slotId)
    {
        try
        {
            // 创建屏幕截图
            Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenTexture.Apply();
            
            // 转换为PNG
            byte[] textureBytes = screenTexture.EncodeToPNG();
            Destroy(screenTexture);
            
            // 保存路径
            string saveDirectory = GetParentSavePath();
            string fileName = $"screenshot_{slotId}.png";
            string fullPath = Path.Combine(saveDirectory, fileName);
            
            // 保存文件
            File.WriteAllBytes(fullPath, textureBytes);
            
            Debug.Log($"截图保存成功: {fullPath}");
            
            // 返回相对路径或文件名（根据你的需求）
            return fileName; // 或者返回 fullPath
            
        }
        catch (Exception e)
        {
            Debug.LogError("截图保存失败: " + e.Message);
            return "";
        }
    }
}