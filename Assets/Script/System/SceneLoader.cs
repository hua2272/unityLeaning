using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private string nextSceneName;
    [SerializeField] private Vector3 spawnPosition;
    
    private Player player;
    private GameDataManager gameDataManager;
    private bool playerInRange;//玩家是否在门范围
    private bool isTransitioning = false;

    private void Start()
    {
        player = PlayerManager.instance.player;
        gameDataManager =  GameDataManager.instance;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        isTransitioning = true;
        if (transitionAnimator != null) transitionAnimator.SetTrigger("Start");// 开始过渡动画
        
        yield return new WaitForSeconds(transitionTime);
        
        bool isSameScene = sceneName == SceneManager.GetActiveScene().name;
        if (!isSameScene)
        {
            gameDataManager.spawnPosition = spawnPosition;// 设置重生位置
            gameDataManager.targetScene = sceneName;
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);// 直接加载场景，确保激活
            asyncLoad.allowSceneActivation = true; // 确保场景激活
            
            while (!asyncLoad.isDone)// 等待加载完成
            {
                yield return null;
            }
        }
        else
        {
            player.transform.position = spawnPosition;                                  //同场景传送则直接移动玩家，不销毁传送门
            if (transitionAnimator != null) transitionAnimator.SetTrigger("End");
            isTransitioning = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false;
    }
    
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.X) && !isTransitioning)
        {
            LoadScene(nextSceneName);
        }
    }
}