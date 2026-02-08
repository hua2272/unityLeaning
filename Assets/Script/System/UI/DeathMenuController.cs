using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathMenuController : MonoBehaviour
{
    [SerializeField] private Animator menuAnimator; 
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    
    private void Start()
    {
        continueButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitToMainMenu);
        menuAnimator.Rebind();                                          // 重置动画状态（防止第一次播放时出现问题）
        menuAnimator.Update(0f);
        menuAnimator.SetBool("ShowMenu", true);                   // 触发动画
        Invoke("EnableButtons", 3f);                   // 延迟启用按钮交互（等待动画完成）
    }
    
    private void EnableButtons()
    {
        continueButton.interactable = true;
        quitButton.interactable = true;
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;                                                     // 重置游戏状态
        menuAnimator.SetBool("ShowMenu", false);                           // 触发关闭动画
        continueButton.interactable = false;                                     // 立即禁用按钮防止多次点击
        quitButton.interactable = false;
        Invoke("DisableCanvas", 1f);                            // 延迟关闭Canvas（等待动画完成）
        string latestSaveFiles = GameSaveManager.instance.GetLatestSaveFiles();
        GameLoadManager.instance.LoadLatestGame(latestSaveFiles);
    }
    
    private void DisableCanvas()
    {
        gameObject.SetActive(false);
    }
    
    private void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        GameLoadManager.instance.OnQuitClicked();
    }
}