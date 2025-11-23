using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentSceneLoader : MonoBehaviour
{
    public static PersistentSceneLoader instance;
    void Awake()
    {
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
    
    void Start()
    {
        if (SceneManager.sceneCount == 1) LoadMainMenu();                                               //如果当前只有Persistent场景，则加载主菜单
    }
    
    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuCoroutine());
    }
    
    private IEnumerator LoadMainMenuCoroutine()
    {
        Debug.Log("加载主菜单场景");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);     //使用异步加载以便显示加载界面
        while (!asyncLoad.isDone)
        {
            // 可以在这里更新加载进度
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));                           //设置新加载的场景为活动场景
        Debug.Log("主菜单加载完成");
    }
}