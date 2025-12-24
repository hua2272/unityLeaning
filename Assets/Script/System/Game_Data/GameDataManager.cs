using System.Collections.Generic;
using System.IO;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance { get; private set; }
    
    public string saveTime = "";
    public string playTime = "";
    public int playerLevel;
    public int playerHealth;
    public int equippedSlotId;
    public string scene;
    public Vector3 playerPosition;
    public string[] inventoryItems;
    public string screenshot;
    public GameData.SerializableDictionary unlockedSkills;
    
    // 新增：当前选择的存档槽位
    private int currentSaveSlot = 0;
    
    private Player player;
    public Vector3 spawnPosition;
    public string targetScene;
    public CinemachineVirtualCamera virtualCamera;
    
    void Awake()
    {
        Debug.Log("<color=#FF0000>-------GameDataManager instance-------</color>");
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        player = PlayerManager.instance.player;
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Persistent") return;
        if (scene.name == targetScene)
        {
            player.transform.position = spawnPosition;
            Debug.Log($"场景 {scene.name} 加载完成");
            Debug.Log($"玩家位置已设置到: {spawnPosition}");
        }
        
        SceneLoader[] allPortals = FindObjectsOfType<SceneLoader>();
        foreach (SceneLoader portal in allPortals)
        {
            if (portal.gameObject.scene.name != scene.name)
                Destroy(portal.gameObject);//todo 清理不属于当前场景的传送门，该功能放哪比较好
        }
        virtualCamera.Follow = player.transform;
    }

    public void DataPersistence(GameData gameData)
    {
        playerLevel =  gameData.playerLevel;
        playerHealth = gameData.playerHealth;
        equippedSlotId = gameData.equippedSlotId;
        scene = gameData.scene;
        playerPosition = gameData.playerPosition;
        inventoryItems = gameData.inventoryItems;
        unlockedSkills = gameData.unlockedSkills;
        saveTime = gameData.saveTime;
        playTime = gameData.playTime;
        currentSaveSlot = gameData.saveSlotId;
        screenshot =  gameData.screenshot;
    }
    
    public Dictionary<string, GameData> LoadAllGameDataFromDirectory(string directoryPath)
    {
        var gameDataMap = new Dictionary<string, GameData>();
    
        // 检查目录是否存在
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogWarning($"目录不存在: {directoryPath}");
            return gameDataMap;
        }
    
        try
        {
            // 获取目录下所有.dat文件
            string[] datFiles = Directory.GetFiles(directoryPath, "*.dat");
        
            Debug.Log($"在目录 {directoryPath} 中找到 {datFiles.Length} 个.dat文件");
        
            foreach (string filePath in datFiles)
            {
                string fileName = Path.GetFileName(filePath); // 只获取文件名，不包括路径
            
                try
                {
                    // 读取文件内容
                    string jsonData = File.ReadAllText(filePath);
                
                    // 解析JSON为GameData对象
                    GameData gameData = JsonUtility.FromJson<GameData>(jsonData);
                
                    if (gameData != null)
                    {
                        // 添加到字典
                        gameDataMap[fileName] = gameData;
                        Debug.Log($"成功加载文件: {fileName}");
                    }
                    else
                    {
                        Debug.LogWarning($"文件 {fileName} 解析失败: JSON数据无效");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"读取文件 {fileName} 时发生错误: {ex.Message}");
                    // 可以选择是否继续处理其他文件
                    // 如果需要继续，这里不要抛出异常
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"读取目录 {directoryPath} 时发生错误: {ex.Message}");
        }
    
        Debug.Log($"总共加载了 {gameDataMap.Count} 个有效的游戏数据文件");
        return gameDataMap;
    }
}