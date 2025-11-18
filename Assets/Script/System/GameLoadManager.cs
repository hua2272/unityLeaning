using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoadManager : MonoBehaviour
{
    public static GameLoadManager instance { get; private set; }
    
    public GameObject player;
    public Vector3 spawnPosition;
    public string targetScene;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景 {scene.name} 加载完成");
        
        // 设置玩家位置
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
        SceneLoader[] allPortals = FindObjectsOfType<SceneLoader>();
        foreach (SceneLoader portal in allPortals)
        {
            if (portal.gameObject.scene.name != scene.name)
            {
                Destroy(portal.gameObject);
            }
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
        else
        {
            Debug.LogWarning("Save file not found");
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
        if (gameData == null)
        {
            Debug.LogError("Failed to parse save data!");
            yield break;
        }
        
        Debug.Log("<color=#FF0000>--------DataPersistenceStart--------</color>");
        
        // 先加载地形数据
        if (GameSaveManager.instance != null && gameData.destroyedTiles != null)
        {
            GameSaveManager.instance.LoadTileStates(gameData.destroyedTiles);
            Debug.Log($"加载了 {gameData.destroyedTiles.Count} 个地形破坏记录");
        }
        
        // 再加载玩家数据
        GameDataManager.instance.DataPersistence(gameData);
        
        SceneManager.LoadScene("Persistent");
        yield return null;
        
        while (SceneManager.GetActiveScene().name != gameData.scene)
        {
            yield return null;
        }
    }
}