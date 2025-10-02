using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoadManager : MonoBehaviour
{
    public static GameLoadManager Instance { get; private set; }
    
    public GameObject player;
    public Vector3 spawnPosition;
    public string targetScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    // 确保在销毁时移除事件监听
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        // TODO 重置玩家数据、关卡状态等
        SceneTransitionManager.Instance.LoadSceneWithFade("GameScene");
    }

    public void LoadGame()
    {
        // todo 多存档管理
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

    //todo FindObjectOfType方法查询效率低，后期优化
    private IEnumerator LoadGameCoroutine(string filePath)
    {
        string jsonData = File.ReadAllText(filePath);
        GameData gameData = JsonUtility.FromJson<GameData>(jsonData);
        if (gameData == null)
        {
            Debug.LogError("Failed to parse save data!");
            yield break;
        }
        Debug.unityLogger.Log("--------DataPersistenceStart---------");
        GameDataManager.instance.DataPersistence(gameData);
        
        //SceneManager.LoadScene("Persistent", LoadSceneMode.Additive);
        SceneTransitionManager.Instance.LoadSceneWithFade(gameData.scene);
        yield return null;                                                              //等待一帧让场景开始加载
        while (SceneManager.GetActiveScene().name != gameData.scene)                    //等待场景完全加载
        {
            yield return null;
        }
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)                                                          //确保玩家对象已生成
        {
            Debug.LogError("Player object not found in the scene!");
            yield break;
        }
        player.transform.position = new Vector3(gameData.playerPosition.x, gameData.playerPosition.y, gameData.playerPosition.z);

        Debug.Log("存档中读取到已装备的武器: " + gameData.equippedSlotId);
        WeaponSlotsManager weaponSlotsManager = FindObjectOfType<WeaponSlotsManager>(true);
        if (weaponSlotsManager != null)
        {
            Debug.Log($"找到WeaponSlotsManager: {weaponSlotsManager.name}");
            weaponSlotsManager.Start();
            weaponSlotsManager.HandleSlotClick(gameData.equippedSlotId);
        }
    }
}