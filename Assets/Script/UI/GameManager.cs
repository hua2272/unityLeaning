using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 避免重复创建
        }
        else
        {
            Instance = this; // 初始化单例
            DontDestroyOnLoad(gameObject); // 跨场景不销毁
        }
    }

    public void StartNewGame()
    {
        Debug.Log("初始化新游戏...");
        // 这里可以重置玩家数据、关卡状态等
    }

    public void LoadGame()
    {
        Debug.Log("加载存档...");
        // 调用 SaveSystem 加载数据
    }
}