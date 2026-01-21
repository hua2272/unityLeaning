using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager instance { get; private set; }

    private GameDataManager gameDataManager;
    private Player player;
    public WeaponSlotsManager weaponSlotsManager;
    private GameData currentGameData = new GameData();
    
    // 截图配置参数
    [Header("截图设置")]
    [SerializeField] private float screenshotOrthographicSize = 5f; // 相机正交大小（视野大小）
    [SerializeField] private int screenshotWidth = 512; // 截图宽度
    [SerializeField] private int screenshotHeight = 512; // 截图高度
    [SerializeField] private LayerMask screenshotLayers; // 要渲染的层级
    [SerializeField] private Color backgroundColor = Color.white; // 背景颜色
    
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
        
        if (screenshotLayers.value == 0)
            screenshotLayers = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ground"));// 设置默认的截图层级（Player和Ground层）
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
        
        currentGameData.destroyedTiles = GetAllDestroyedTiles();// 保存地形数据
        
        currentGameData.screenshot = CaptureScreenshot(slotId);// 保存截图
        
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
    
    public static List<string> GetAllSaveFiles()
    {
        List<string> saveFiles = new List<string>();
        try
        {
            string directoryPath = GetParentSavePath();
            
            string[] files = Directory.GetFiles(directoryPath, "*.dat");// 搜索所有.dat文件
            foreach (string file in files)// 将文件路径添加到列表中
            {
                saveFiles.Add(file);
            }
            Debug.Log($"在目录 {directoryPath} 中找到 {saveFiles.Count} 个保存文件");
        }
        catch (Exception e)
        {
            Debug.LogError($"获取保存文件列表失败: {e.Message}");
        }
        return saveFiles;
    }

    public string GetLatestSaveFiles()
    {
        string directoryPath = GetParentSavePath();
        string[] saveFiles = Directory.GetFiles(directoryPath, "*.dat");    // 搜索所有.dat文件
        if (saveFiles.Length == 0) return "";
        
        Dictionary<string, DateTime> fileTimes = new Dictionary<string, DateTime>();    // 使用字典存储文件路径和修改时间
        foreach (string filePath in saveFiles)                                          // 获取每个文件的最后修改时间
        {
            try
            {
                if (File.Exists(filePath))
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                    fileTimes[filePath] = lastWriteTime;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"读取文件时间失败 {filePath}: {e.Message}");
            }
        }
        
        string latestFilePath = null;
        DateTime latestTime = DateTime.MinValue;
        foreach (var kvp in fileTimes)                                                   // 找到修改时间最近的文件
        {
            if (kvp.Value > latestTime)
            {
                latestTime = kvp.Value;
                latestFilePath = kvp.Key;
            }
        }
        return latestFilePath;
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
            Vector3 playerPosition = player.transform.position;// 获取玩家当前位置
            
            // 创建一个新的相机对象
            GameObject screenshotCameraObj = new GameObject("ScreenshotCamera");
            Camera screenshotCamera = screenshotCameraObj.AddComponent<Camera>();
            
            // 设置相机属性
            screenshotCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, -10f);
            screenshotCamera.orthographic = true;
            screenshotCamera.orthographicSize = screenshotOrthographicSize;
            screenshotCamera.cullingMask = screenshotLayers;
            screenshotCamera.clearFlags = CameraClearFlags.SolidColor;
            screenshotCamera.backgroundColor = backgroundColor;
            screenshotCamera.enabled = false; // 不启用相机，手动渲染
            
            // 创建RenderTexture
            RenderTexture renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
            screenshotCamera.targetTexture = renderTexture;
            screenshotCamera.Render();// 渲染一帧到RenderTexture
            
            // 从RenderTexture读取数据到Texture2D
            RenderTexture.active = renderTexture;
            Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
            screenshot.Apply();
            
            // 清理资源
            RenderTexture.active = null;
            screenshotCamera.targetTexture = null;
            Destroy(renderTexture);
            Destroy(screenshotCameraObj);
            byte[] textureBytes = screenshot.EncodeToPNG();// 转换为PNG
            Destroy(screenshot);
            
            // 保存路径（不使用下划线，文件生成会有问题）
            string saveDirectory = GetParentSavePath();
            string fileName = $"screenshot{slotId}.png";
            string fullPath = Path.Combine(saveDirectory, fileName);
            File.WriteAllBytes(fullPath, textureBytes);// 保存文件
            
            Debug.Log($"截图保存成功: {fullPath}");
            Debug.Log($"截图位置：玩家位置({playerPosition.x:F2}, {playerPosition.y:F2})，视野大小：{screenshotOrthographicSize}");
            
            return fullPath; //返回文件全路径
        }
        catch (Exception e)
        {
            Debug.LogError("截图保存失败: " + e.Message);
            return "";
        }
    }
}