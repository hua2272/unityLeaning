using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private string nextSceneName; // 目标场景名
    
    private bool isTransitioning = false;
    // 玩家状态数据
    public Vector3 spawnPosition;
    public int playerHealth;
    public int playerScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 跨场景持久化
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        
        if (transitionAnimator != null) // 播放转场动画（如果有）
            transitionAnimator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName); // 异步加载场景
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f) // 加载进度到90%时等待
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        isTransitioning = false;
        
        
        EnsurePlayerExists(); // 确保新场景的玩家生成// 加载完成后的逻辑（如初始化游戏）
        Debug.Log($"场景 {sceneName} 加载完成！");
    }
    
    private void EnsurePlayerExists()
    {
        // 检查新场景是否有玩家
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null)
            {
                spawner.SpawnPlayer();
                Debug.Log("已在新场景生成玩家");
            }
            else
            {
                Debug.LogError("未找到PlayerSpawner！");
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.CompareTag("Player") && Input.GetKeyDown(KeyCode.F)) // 确保碰撞对象是玩家
        if (collision.CompareTag("Player")) // 确保碰撞对象是玩家
        {
            spawnPosition = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;  // 重生点的位置
            LoadScene(nextSceneName); // 切换场景
        }
    }
}