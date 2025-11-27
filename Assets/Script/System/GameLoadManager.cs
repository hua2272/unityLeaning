using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoadManager : MonoBehaviour
{
    public static GameLoadManager instance { get; private set; }
    
    public Player player;
    public Vector3 spawnPosition;
    public string targetScene;

    private void Awake()
    {
        Debug.Log("<color=#FF0000>-------GameLoadManager instance-------</color>");
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
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        player = PlayerManager.instance.player;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == targetScene)
        {
            player.transform.position = spawnPosition;// 设置玩家位置
            Debug.Log($"场景 {scene.name} 加载完成");
            Debug.Log($"玩家位置已设置到: {spawnPosition}");
        }
        
        SceneLoader[] allPortals = FindObjectsOfType<SceneLoader>();
        foreach (SceneLoader portal in allPortals)
        {
            if (portal.gameObject.scene.name != scene.name)
                Destroy(portal.gameObject);// 清理不属于当前场景的传送门
        }
    }

    public void StartNewGame()
    {
        // 新游戏时清空地形破坏记录
        if (GameSaveManager.instance != null)
        {
            GameSaveManager.instance.ClearAllTileStates();
        }
        SceneTransitionManager.Instance.LoadSceneWithFade("GameScene");
    }

    public void LoadGame()
    {
        if (GameSaveManager.DoesSaveExist())
        {
            StartCoroutine(LoadGameCoroutine(GameSaveManager.GetSavePath()));
        }
    }
    
    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    private IEnumerator LoadGameCoroutine(string filePath)
    {
        string jsonData = File.ReadAllText(filePath);
        GameData gameData = JsonUtility.FromJson<GameData>(jsonData);
        if (gameData == null) yield break;
        Debug.Log("<color=#FF0000>--------DataPersistence Start--------</color>");
        
        if (GameSaveManager.instance != null && gameData.destroyedTiles != null)// 先加载地形数据
        {
            GameSaveManager.instance.LoadTileStates(gameData.destroyedTiles);
            Debug.Log($"加载了 {gameData.destroyedTiles.Count} 个地形破坏记录");
        }
        
        GameDataManager.instance.DataPersistence(gameData);// 再加载玩家数据
        SceneManager.LoadScene("harbor");
        player.transform.position = new Vector3(gameData.playerPosition.x, gameData.playerPosition.y, gameData.playerPosition.z);
        yield return null;
    }
}