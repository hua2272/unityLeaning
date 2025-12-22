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
    public AudioClip  background;
    private AudioManager audioManager;
    
    private bool mainMenuLoaded = false;
    private bool waitingForInput = false;
    private UILangue uiLangue;
    private UIManager uiManager;

    private int langueType;
    
    void Awake()
    {
        introCanvas.gameObject.SetActive(true);
        SetCameraToTargetPosition();                                             //设置相机到指定位置
    }
    
    void Start()
    {
        audioManager = AudioManager.instance;
        uiLangue = UILangue.instance;
        uiManager = UIManager.instance;
        StartCoroutine(StartIntroSequence());                             //开始闪烁文本和等待输入
        audioManager.PlayBackgroundMusic(background, true);                 //播放背景音乐
        uiManager.SetUIVisibility(UIGroup.PlayerStatus, false, 0);
        prompt.text = uiLangue.Content(3);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && waitingForInput && !mainMenuLoaded)
        {
            audioManager.PlayUISound(UISoundType.Click);
            buttonPanel.SetActive(true);
            title.gameObject.SetActive(true);
            prompt.gameObject.SetActive(false);
            title.text = uiLangue.Content(1);
        }
    }
    
    private void SetCameraToTargetPosition()
    {
        virtualCamera.Follow = null;                                            //禁用相机跟随，让它停留在指定位置
        virtualCamera.transform.position = cameraTargetPosition.position;
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