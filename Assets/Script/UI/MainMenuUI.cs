using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnNewGameClicked()
    {
        GameLoadManager.Instance.StartNewGame(); // 通知 GameManager 初始化新游戏
        //SceneLoader.Instance.LoadScene("GameScene"); // 加载游戏场景
        SceneTransitionManager.Instance.LoadSceneWithFade("GameScene");
    }

    //todo 若没有存档文件则提示没有存档
    public void OnLoadGameClicked()
    {
        if (GameSaveManager.DoesSaveExist()) // 检查是否有存档
        {
            GameLoadManager.Instance.LoadGame(); // 通知 GameManager 加载存档
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