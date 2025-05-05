using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnNewGameClicked()
    {
        GameManager.Instance.StartNewGame(); // 通知 GameManager 初始化新游戏
        SceneLoader.Instance.LoadScene("GameScene"); // 加载游戏场景
    }

    public void OnLoadGameClicked()
    {
        if (GameSaveManager.DoesSaveExist()) // 检查是否有存档
        {
            GameManager.Instance.LoadGame(); // 通知 GameManager 加载存档
            SceneLoader.Instance.LoadScene("GameScene"); // 加载游戏场景
        }
        else
        {
            Debug.LogWarning("没有找到存档！");
            // 可以在这里显示 UI 提示
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
}