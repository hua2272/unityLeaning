using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1f;

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
        // 播放转场动画（如果有）
        if (transitionAnimator != null)
            transitionAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 加载完成后的逻辑（如初始化游戏）
        Debug.Log($"场景 {sceneName} 加载完成！");
    }
}