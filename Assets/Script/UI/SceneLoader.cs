using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private string nextSceneName;
    [SerializeField] private Vector3 spawnPosition;
    
    private bool playerInRange;
    private bool isTransitioning = false;

    // 使用字典保存每个场景的出生位置
    public static Dictionary<string, Vector3> sceneSpawnPositions = new Dictionary<string, Vector3>();
    
    // 当前场景的玩家状态
    private Vector3 currentSpawnPosition;

    private void Start()
    {
        // 初始化当前出生位置
        currentSpawnPosition = spawnPosition;
    }

    public void LoadScene(string sceneName)
    {
        // 保存当前场景的出生位置
        if (!sceneSpawnPositions.ContainsKey(SceneManager.GetActiveScene().name))
        {
            sceneSpawnPositions.Add(SceneManager.GetActiveScene().name, currentSpawnPosition);
        }
        
        // 保存目标位置到字典
        sceneSpawnPositions[sceneName] = spawnPosition;
        
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        isTransitioning = true;
        
        if (transitionAnimator != null)
            transitionAnimator.SetTrigger("Start");
        
        yield return new WaitForSeconds(transitionTime);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
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
        if (playerInRange && Input.GetKeyDown(KeyCode.F) && !isTransitioning)
        {
            LoadScene(nextSceneName);
        }
    }
}