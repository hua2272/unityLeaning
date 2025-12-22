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
    public GameObject buttonPanel;
    
    [Header("Camera Control")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform cameraTargetPosition;
    
    [Header("Audio Settings")]
    public AudioClip  background;
    private AudioManager audioManager;
    
    private bool mainMenuLoaded = false;
    private bool waitingForInput = false;
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
        uiManager = UIManager.instance;
        StartCoroutine(StartIntroSequence());                             //开始闪烁文本和等待输入
        audioManager.PlayBackgroundMusic(background, true);                 //播放背景音乐
        uiManager.SwitchScene(UIPreset.PressStart);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && waitingForInput && !mainMenuLoaded)
        {
            audioManager.PlayUISound(UISoundType.Click);
            buttonPanel.SetActive(true);
            uiManager.SwitchScene(UIPreset.Title);
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
        waitingForInput = true;
    }
}