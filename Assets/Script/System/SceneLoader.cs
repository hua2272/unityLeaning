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
    private bool playerInRange;//玩家是否在门范围
    private bool isTransitioning = false;

    private void Start()
    {
        player = PlayerManager.instance.player;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        isTransitioning = true;
        
        if (transitionAnimator != null)
            transitionAnimator.SetTrigger("Start");
        
        yield return new WaitForSeconds(transitionTime);
        
        bool isSameScene = sceneName == SceneManager.GetActiveScene().name;             //判断是否是同场景传送
        
        if (!isSameScene)
        {
            GameLoadManager.instance.spawnPosition = spawnPosition;                    //不同场景传送需持久化位置信息，并销毁传送门
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);          //异步加载新场景
            asyncLoad.allowSceneActivation = false;
            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }
        }
        else
        {
            player.transform.position = spawnPosition;                                  //同场景传送则直接移动玩家，不销毁传送门
            if (transitionAnimator != null)
                transitionAnimator.SetTrigger("End");                              //结束过渡动画
        }
        isTransitioning = false;
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