using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Cinemachine;

public class Prelude : MonoBehaviour
{
    [Header("UI References")]
    public Canvas introCanvas;
    public Animator canvasAnimator;
    public TextMeshProUGUI prompt;
    public TextMeshProUGUI title;
    public GameObject buttonPanel;
    
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
    
    void Awake()
    {
        InitializeAudioSources();// 初始化音频源
        introCanvas.gameObject.SetActive(true);
        SetCameraToTargetPosition();// 设置相机到指定位置
    }
    
    void Start()
    {
        StartCoroutine(StartIntroSequence());       //开始闪烁文本和等待输入
        backgroundMusicSource.Play();                      //播放背景音乐
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && waitingForInput && !mainMenuLoaded)
        {
            sfxSource.Play();
            buttonPanel.SetActive(true);
            title.gameObject.SetActive(true);
            prompt.gameObject.SetActive(false);
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
        prompt.gameObject.SetActive(true);
        bool isVisible = true;
        float blinkRate = 0.5f;
        
        while (waitingForInput && !mainMenuLoaded)
        {
            isVisible = !isVisible;
            prompt.enabled = isVisible;
            yield return new WaitForSeconds(blinkRate);
        }
    }
}