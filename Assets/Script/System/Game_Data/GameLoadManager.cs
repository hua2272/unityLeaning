using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoadManager : MonoBehaviour
{
    public static GameLoadManager instance { get; private set; }
    
    private Player player;
    private GameSaveManager gameSaveManager;
    private GameStateManager gameStateManager;
    private AudioManager audioManager;
    private UIManager uiManager;
    private string currentSceneName;

    private void Awake()
    {
        Debug.Log("<color=#FF0000>-------GameLoadManager instance-------</color>");
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
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        player = PlayerManager.instance.player;
        gameSaveManager =  GameSaveManager.instance;
        gameStateManager = GameStateManager.instance;
        audioManager = AudioManager.instance;
        uiManager = UIManager.instance;
        currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
        
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = scene.name;
        if (newSceneName == currentSceneName) return;
        currentSceneName = newSceneName;
        audioManager.StopBackgroundMusic();
        uiManager.SwitchScene(UIPreset.Normal);
    }

    public void StartNewGame()
    {
        gameSaveManager.ClearAllTileStates();// 新游戏时清空地形破坏记录
        SceneTransitionManager.Instance.LoadSceneWithFade("GameScene");
    }

    public void LoadGame(int slotId)
    {
        string savePath = GameSaveManager.GetSavePath(slotId);
        if (File.Exists(savePath))
        {
            StartCoroutine(LoadGameCoroutine(savePath));
        }
    }
    
    public void LoadLatestGame(string latestFilePath)
    {
        StartCoroutine(LoadGameCoroutine(latestFilePath));
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
        gameStateManager.CurrentState = GameState.Normal;
        player.transform.position = new Vector3(gameData.playerPosition.x, gameData.playerPosition.y, gameData.playerPosition.z);
        yield return null;
    }
}