using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Cinemachine;

public class PersistentSceneLoader : MonoBehaviour
{
    public static PersistentSceneLoader instance;
    
    [Header("UI References")]
    public Canvas introCanvas;
    public Animator canvasAnimator;
    public TextMeshProUGUI pressCText;
    
    [Header("Camera Control")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform cameraTargetPosition;
    
    [Header("Audio Settings")]
    public AudioSource backgroundMusicSource;
    public AudioSource sfxSource;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    private bool mainMenuLoaded = false;
    private bool waitingForInput = false;
    private Transform originalFollowTarget;
    
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
        
        InitializeAudioSources();// 初始化音频源
        
        originalFollowTarget = virtualCamera.Follow;
        introCanvas.gameObject.SetActive(true);
        
        SetCameraToTargetPosition();// 设置相机到指定位置
    }
    
    void Start()
    {
        StartCoroutine(StartIntroSequence());// 开始闪烁文本和等待输入
        backgroundMusicSource.Play();// 播放背景音乐
    }
    
    void Update()
    {
        // 检测C键按下且主菜单尚未加载
        if (Input.GetKeyDown(KeyCode.C) && waitingForInput && !mainMenuLoaded)
        {
            sfxSource.Play();
            StartCoroutine(LoadMainMenuCoroutine());
        }
    }
    
    private void InitializeAudioSources()
    {
        // 配置背景音乐源
        backgroundMusicSource.loop = true;
        backgroundMusicSource.volume = musicVolume;
        backgroundMusicSource.spatialBlend = 0f; // 2D声音
        // 配置音效源
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.spatialBlend = 0f; // 2D声音
    }
    
    private IEnumerator FadeMusicCoroutine(float fromVolume, float toVolume, float duration)
    {
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            backgroundMusicSource.volume = Mathf.Lerp(fromVolume, toVolume, timer / duration);
            yield return null;
        }
        backgroundMusicSource.volume = toVolume;
    }
    
    private void SetCameraToTargetPosition()
    {
        virtualCamera.Follow = null;// 禁用相机跟随，让它停留在指定位置
        virtualCamera.transform.position = cameraTargetPosition.position;// 将相机移动到目标位置
        virtualCamera.transform.rotation = cameraTargetPosition.rotation;
    }
    
    private IEnumerator StartIntroSequence()
    {
        introCanvas.gameObject.SetActive(true);
        canvasAnimator.SetTrigger("StartIntro");
        yield return new WaitForSeconds(1f);
        StartCoroutine(BlinkText());
        waitingForInput = true;
    }
    
    private IEnumerator BlinkText()
    {
        if (pressCText == null) yield break;
        
        pressCText.gameObject.SetActive(true);
        bool isVisible = true;
        float blinkRate = 0.5f;
        
        while (waitingForInput && !mainMenuLoaded)
        {
            isVisible = !isVisible;
            pressCText.enabled = isVisible;
            yield return new WaitForSeconds(blinkRate);
        }
    }
    
    private IEnumerator LoadMainMenuCoroutine()
    {
        mainMenuLoaded = true;
        waitingForInput = false;
        Debug.Log("加载主菜单场景");
        
        StartCoroutine(FadeMusicCoroutine(backgroundMusicSource.volume, 0f, 1f));// 淡出当前音乐
        yield return new WaitForSeconds(0.3f);
        
        canvasAnimator.SetTrigger("ExitIntro");// 播放退出动画
        yield return new WaitForSeconds(0.5f);
        
        introCanvas.gameObject.SetActive(false);// 隐藏intro canvas
        virtualCamera.Follow = originalFollowTarget;// 恢复相机跟随
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);// 加载主菜单场景
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
        Debug.Log("主菜单加载完成");
    }
}