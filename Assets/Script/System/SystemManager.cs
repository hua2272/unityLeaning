using UnityEngine;
using UnityEngine.UI;

public class SystemManager : MonoBehaviour
{
    public static SystemManager instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject panel; // 总面板
    [SerializeField] private GameObject mainPanel; // 主要面板
    [SerializeField] private GameObject[] subPanels; // 二级面板
    
    [Header("Navigation Settings")]
    [SerializeField] private int defaultSubPanelIndex = 0; // 默认二级面板索引
    
    private int currentSubPanelIndex = -1; // -1表示在主面板
    private bool isInSubPanel = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializePanels();
    }

    private void InitializePanels()
    {
        // 初始化所有面板为未激活状态
        if (mainPanel != null)
            mainPanel.SetActive(false);
            
        foreach (var subPanel in subPanels)
        {
            if (subPanel != null)
                subPanel.SetActive(false);
        }
        
        panel.SetActive(false);
        isInSubPanel = false;
        currentSubPanelIndex = -1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    // 处理ESC键逻辑
    private void HandleEscapeKey()
    {
        if (isInSubPanel)
        {
            // 如果在二级面板，返回主面板
            ReturnToMainPanel();
        }
        else
        {
            // 如果在主面板，切换系统面板显示/隐藏
            ToggleSystemPanel();
        }
    }

    // 切换系统面板的显示/隐藏
    public void ToggleSystemPanel()
    {
        bool isActive = !panel.activeSelf;
        panel.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;
        
        // 如果打开面板，显示主面板
        if (isActive)
        {
            ShowMainPanel();
        }
        else
        {
            // 关闭面板时重置状态
            isInSubPanel = false;
            currentSubPanelIndex = -1;
        }
    }

    // 显示主面板
    public void ShowMainPanel()
    {
        // 隐藏所有二级面板
        foreach (var subPanel in subPanels)
        {
            if (subPanel != null)
                subPanel.SetActive(false);
        }
        
        // 显示主面板
        if (mainPanel != null)
            mainPanel.SetActive(true);
            
        isInSubPanel = false;
        currentSubPanelIndex = -1;
    }

    // 进入二级面板
    public void EnterSubPanel(int subPanelIndex)
    {
        if (subPanelIndex < 0 || subPanelIndex >= subPanels.Length) return;
        
        // 隐藏主面板
        if (mainPanel != null)
            mainPanel.SetActive(false);
        
        // 显示指定的二级面板
        if (subPanels[subPanelIndex] != null)
        {
            subPanels[subPanelIndex].SetActive(true);
            currentSubPanelIndex = subPanelIndex;
            isInSubPanel = true;
        }
    }

    // 返回到主面板
    public void ReturnToMainPanel()
    {
        ShowMainPanel();
    }

    // 关闭系统面板
    public void CloseSystemPanel()
    {
        panel.SetActive(false);
        Time.timeScale = 1;
        
        // 重置状态
        isInSubPanel = false;
        currentSubPanelIndex = -1;
    }

    // 获取当前是否在二级面板
    public bool IsInSubPanel()
    {
        return isInSubPanel;
    }

    // 获取当前二级面板索引
    public int GetCurrentSubPanelIndex()
    {
        return currentSubPanelIndex;
    }
}