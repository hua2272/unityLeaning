using UnityEngine;
using UnityEngine.UI;

public class SystemManager : MonoBehaviour
{
    public static SystemManager instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject panel; // 总面板
    [SerializeField] private GameObject[] tabPanels; // 各个页签对应的面板
    [SerializeField] private Button[] tabButtons; // 页签按钮
    
    [Header("Tab Settings")]
    [SerializeField] private int defaultTabIndex = 0; // 默认打开的页签
    
    private int currentTabIndex = 0;

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
        InitializeTabs();
    }

    private void InitializeTabs()
    {
        // 初始化所有页签面板为未激活状态
        foreach (var tabPanel in tabPanels)
        {
            if (tabPanel != null)
                tabPanel.SetActive(false);
        }
        
        // 绑定按钮点击事件
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int tabIndex = i; // 重要：创建局部变量避免闭包问题
            if (tabButtons[i] != null)
            {
                tabButtons[i].onClick.AddListener(() => OnTabButtonClicked(tabIndex));
            }
        }
        
        panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSystemPanel();
        }
    }

    // 切换系统面板的显示/隐藏
    public void ToggleSystemPanel()
    {
        bool isActive = !panel.activeSelf;
        panel.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;
        
        // 如果打开面板，显示默认页签
        if (isActive && !IsAnyTabActive())
        {
            SwitchTab(defaultTabIndex);
        }
    }

    // 检查是否有任何页签是激活状态
    private bool IsAnyTabActive()
    {
        foreach (var tabPanel in tabPanels)
        {
            if (tabPanel != null && tabPanel.activeSelf)
                return true;
        }
        return false;
    }

    // 页签按钮点击事件
    private void OnTabButtonClicked(int tabIndex)
    {
        SwitchTab(tabIndex);
    }

    // 切换页签
    public void SwitchTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabPanels.Length) return;
        
        // 隐藏所有页签面板
        for (int i = 0; i < tabPanels.Length; i++)
        {
            if (tabPanels[i] != null)
                tabPanels[i].SetActive(false);
        }
        
        // 显示选中的页签面板
        if (tabPanels[tabIndex] != null)
        {
            tabPanels[tabIndex].SetActive(true);
            currentTabIndex = tabIndex;
            
            // 更新按钮状态
            UpdateTabButtonsVisual(tabIndex);
        }
    }

    // 更新页签按钮的视觉状态
    private void UpdateTabButtonsVisual(int activeTabIndex)
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] != null)
            {
                // 改变按钮颜色来显示激活状态
                var colors = tabButtons[i].colors;
                colors.normalColor = (i == activeTabIndex) ? Color.white : Color.gray;
                tabButtons[i].colors = colors;
                
                // 可选：改变按钮的缩放
                tabButtons[i].transform.localScale = (i == activeTabIndex) ? 
                    Vector3.one * 1.1f : Vector3.one;
            }
        }
    }

    // 关闭系统面板
    public void CloseSystemPanel()
    {
        panel.SetActive(false);
        Time.timeScale = 1;
    }

    // 获取当前激活的页签索引
    public int GetCurrentTabIndex()
    {
        return currentTabIndex;
    }
}