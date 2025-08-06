using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    
    [System.Serializable]
    public class GameData
    {
        public int playerLevel;
        public float playerHealth;
        public Vector3 playerPosition;
        public string[] inventoryItems;
    }
    public GameObject player;
    public static GameManager Instance { get; private set; }
    
    public Vector3 spawnPosition;
    public string targetScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 避免重复创建
            return;
        }
        
        Instance = this; // 初始化单例
        DontDestroyOnLoad(gameObject); // 跨场景不销毁
        
        // 添加场景加载事件监听
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    // 添加场景加载完成后的处理
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景 {scene.name} 加载完成");
        
        // 设置玩家位置（如果是从传送门进入）
        if (scene.name == targetScene)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = spawnPosition;
                Debug.Log($"玩家位置已设置到: {spawnPosition}");
            }
        }
        
        // 清理不属于当前场景的传送门
        CleanUpPortals(scene.name);
    }
    
    // 清理不属于当前场景的传送门
    private void CleanUpPortals(string currentScene)
    {
        SceneLoader[] allPortals = FindObjectsOfType<SceneLoader>();
        foreach (SceneLoader portal in allPortals)
        {
            if (portal.gameObject.scene.name != currentScene)
            {
                Destroy(portal.gameObject);
            }
        }
    }
    
    // 确保在销毁时移除事件监听
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartNewGame()
    {
        Debug.Log("初始化新游戏...");
        // 这里可以重置玩家数据、关卡状态等
    }

    public void LoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }

    private IEnumerator LoadGameCoroutine()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("GameScene");
        yield return null;  //等待一帧让场景开始加载
        while (SceneManager.GetActiveScene().name != "GameScene")  //等待场景完全加载
        {
            yield return null;
        }
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)  //确保玩家对象已生成
        {
            Debug.LogError("Player object not found in the scene!");
            yield break;
        }
        string filePath = GetSavePath();
        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                GameData gameData = JsonUtility.FromJson<GameData>(jsonData);
                if (gameData == null)
                {
                    Debug.LogError("Failed to parse save data!");
                    yield break;
                }
                Vector3 savedPosition = new Vector3(
                    gameData.playerPosition.x,
                    gameData.playerPosition.y,
                    gameData.playerPosition.z
                );
                player.transform.position = savedPosition;
                Debug.Log("Player position loaded: " + savedPosition);
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading save file: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("Save file not found at: " + filePath);
        }
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