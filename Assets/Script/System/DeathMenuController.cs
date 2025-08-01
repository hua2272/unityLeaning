using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathMenuController : MonoBehaviour
{
    [SerializeField] private GameObject deathMenuCanvas; // 引用整个Canvas对象
    [SerializeField] private Animator menuAnimator; 
    //[SerializeField] private AudioSource deathAudio;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    
    private void Awake()
    {
        // 确保开始时菜单是隐藏的
        deathMenuCanvas.SetActive(false);
        continueButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitToMainMenu);
        
        // 初始禁用按钮交互
        continueButton.interactable = false;
        quitButton.interactable = false;
    }
    
    public void ShowDeathMenu()
    {
        // 激活整个Canvas对象
        deathMenuCanvas.SetActive(true);
        // 重置动画状态（防止第一次播放时出现问题）
        menuAnimator.Rebind();
        menuAnimator.Update(0f);
        // 触发动画
        menuAnimator.SetBool("ShowMenu", true);
        // 播放死亡音效
        //deathAudio.Play();
        // 延迟启用按钮交互（等待动画完成）
        Invoke("EnableButtons", 3f);
    }
    
    private void EnableButtons()
    {
        continueButton.interactable = true;
        quitButton.interactable = true;
    }
    
    public void ResumeGame()
    {
        // 重置游戏状态
        Time.timeScale = 1f;
        // 触发关闭动画
        menuAnimator.SetBool("ShowMenu", false);
        // 立即禁用按钮防止多次点击
        continueButton.interactable = false;
        quitButton.interactable = false;
        // 延迟关闭Canvas（等待动画完成）
        Invoke("DisableCanvas", 1f);
    }
    
    private void DisableCanvas()
    {
        deathMenuCanvas.SetActive(false);
    }
    
    private void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}