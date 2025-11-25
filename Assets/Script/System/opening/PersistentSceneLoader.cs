using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Cinemachine; // 添加Cinemachine命名空间

public class PersistentSceneLoader : MonoBehaviour
{
    public static PersistentSceneLoader instance;
    
    [Header("UI References")]
    public Canvas introCanvas;
    public Animator canvasAnimator;
    public TextMeshProUGUI pressCText;
    
    [Header("Camera Control")]
    public CinemachineVirtualCamera virtualCamera; // 引用Cinemachine虚拟相机
    public Transform cameraTargetPosition; // 相机停留的目标位置
    
    private bool mainMenuLoaded = false;
    private bool waitingForInput = false;
    private Transform originalFollowTarget; // 保存原始的跟随目标
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 保存原始的跟随目标
        if (virtualCamera != null)
        {
            originalFollowTarget = virtualCamera.Follow;
        }
        
        // 确保Canvas在Awake中正确设置
        if (introCanvas != null)
        {
            introCanvas.gameObject.SetActive(true);
        }
        
        // 设置相机到指定位置
        SetCameraToTargetPosition();
    }
    
    void Start()
    {
        // 开始闪烁文本和等待输入
        StartCoroutine(StartIntroSequence());
    }
    
    void Update()
    {
        // 检测C键按下且主菜单尚未加载
        if (Input.GetKeyDown(KeyCode.C) && waitingForInput && !mainMenuLoaded)
        {
            LoadMainMenu();
        }
    }
    
    private void SetCameraToTargetPosition()
    {
        if (virtualCamera != null && cameraTargetPosition != null)
        {
            // 禁用相机跟随，让它停留在指定位置
            virtualCamera.Follow = null;
            
            // 将相机移动到目标位置
            virtualCamera.transform.position = cameraTargetPosition.position;
            virtualCamera.transform.rotation = cameraTargetPosition.rotation;
        }
    }
    
    private void RestoreCameraFollow()
    {
        if (virtualCamera != null && originalFollowTarget != null)
        {
            // 恢复相机跟随
            virtualCamera.Follow = originalFollowTarget;
        }
    }
    
    private IEnumerator StartIntroSequence()
    {
        introCanvas.gameObject.SetActive(true);
        canvasAnimator.SetTrigger("StartIntro");
        yield return new WaitForSeconds(1f);// 等待动画播放一段时间
        StartCoroutine(BlinkText());// 开始闪烁文本
        waitingForInput = true;// 设置等待输入状态
    }
    
    private IEnumerator BlinkText()
    {
        if (pressCText == null) yield break;
        
        pressCText.gameObject.SetActive(true);
        bool isVisible = true;
        float blinkRate = 0.5f; // 闪烁频率
        
        while (waitingForInput && !mainMenuLoaded)
        {
            isVisible = !isVisible;
            pressCText.enabled = isVisible;
            yield return new WaitForSeconds(blinkRate);
        }
    }
    
    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuCoroutine());
    }
    
    private IEnumerator LoadMainMenuCoroutine()
    {
        mainMenuLoaded = true;
        waitingForInput = false;
        
        Debug.Log("加载主菜单场景");
        
        // 播放退出动画（如果有）
        if (canvasAnimator != null)
        {
            canvasAnimator.SetTrigger("ExitIntro");
            yield return new WaitForSeconds(0.5f); // 等待动画完成
        }
        
        // 隐藏intro canvas
        if (introCanvas != null)
        {
            introCanvas.gameObject.SetActive(false);
        }
        
        // 恢复相机跟随
        RestoreCameraFollow();
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
        Debug.Log("主菜单加载完成");
    }
}