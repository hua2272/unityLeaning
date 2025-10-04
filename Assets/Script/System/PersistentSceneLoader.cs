using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSceneLoader : MonoBehaviour
{
    public static PersistentSceneLoader instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeManagers()
    {
        // 初始化所有管理器
        Debug.Log("-----------InitializeManagers-----------");
    }
    
    // public void LoadGame()
    // {
    //     SceneManager.LoadScene();
    // }
}