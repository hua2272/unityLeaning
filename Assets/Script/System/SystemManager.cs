using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SystemManager : MonoBehaviour
{
    public static SystemManager instance { get; private set; }
    private PlayerInputManager playerInputManager;
    
    [Header("UI References")]
    [SerializeField] private GameObject panel;                                       // 总面板
    [SerializeField] private GameObject mainPanel;                                   // 主要面板
    [SerializeField] private GameObject[] subPanels;                                 // 二级面板
                    
    [Header("Navigation Settings")]
    [SerializeField] private int defaultSubPanelIndex = 0;                           // 默认二级面板索引
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    
    public int globalOptionIndex { get; private set; } = 0;                              // 全局选项索引
    
    // 分面板按钮管理
    private List<Button> mainPanelButtons = new List<Button>();
    private Dictionary<GameObject, List<Button>> subPanelButtons = new Dictionary<GameObject, List<Button>>();
    private List<Button> currentPanelButtons = new List<Button>();
    private int currentButtonIndex = 0;
    
    // 面板层级管理
    private enum PanelLevel { Main, Sub }
    private PanelLevel currentPanelLevel = PanelLevel.Main;
    private int currentSubPanelIndex = -1;
    
    [Header("Scroll Settings")]
    [SerializeField] private float thresholdTop = 5;                                 //滚动阈值（距离顶部/底部的按钮数量）
    [SerializeField] private float scrollStep = 100f;                                //每次滚动的距离

    // ScrollRect 管理
    private Dictionary<GameObject, ScrollRect> panelScrollRects = new Dictionary<GameObject, ScrollRect>();
    private ScrollRect currentScrollRect;
    
    private bool navigationEnabled = true;
    private bool isInRebindingProcess = false; // 新增：标记是否在重绑定过程中

    // 按钮文本管理
    private Dictionary<Button, List<TextMeshProUGUI>> buttonTexts = new Dictionary<Button, List<TextMeshProUGUI>>();

    // 新增：左右方向键事件
    public event Action<int> OnOptionChanged; // 参数：方向（-1左，1右）

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
        CollectPanelScrollRects(); // 收集面板的ScrollRect
        CollectPanelButtons();
        FindAllButtonTexts();
    }

    private void Start()
    {
        playerInputManager = PlayerInputManager.instance;
        playerInputManager.OnButtonsCreated.AddListener(OnInputButtonsCreated);
    }
    
    private void CollectPanelScrollRects()
    {
        panelScrollRects.Clear();
        ScrollRect scrollRectMain = mainPanel.GetComponentInChildren<ScrollRect>();
        if (scrollRectMain != null) 
            panelScrollRects[mainPanel] = scrollRectMain;
        
        foreach (var subPanel in subPanels)
        {
            ScrollRect scrollRect = subPanel.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
                panelScrollRects[subPanel] = scrollRect;
        }
        UpdateCurrentScrollRect();
    }
    
    private void UpdateCurrentScrollRect()                          // 更新当前ScrollRect
    {
        GameObject currentPanel = null;
        currentScrollRect = null;
        if (currentPanelLevel == PanelLevel.Main)
            currentPanel =  mainPanel;
        if (currentPanelLevel == PanelLevel.Sub && currentSubPanelIndex >= 0 && currentSubPanelIndex < subPanels.Length)
            currentPanel =  subPanels[currentSubPanelIndex];
        
        if (currentPanel != null && panelScrollRects.ContainsKey(currentPanel))
            currentScrollRect = panelScrollRects[currentPanel];
    }

    // 当输入按钮创建完成后调用
    private void OnInputButtonsCreated()
    {
        // 重新收集所有按钮
        CollectPanelButtons();
        FindAllButtonTexts();
    
        // 如果当前在按键设置面板，更新选择
        if (currentPanelLevel == PanelLevel.Sub)
        {
            UpdateCurrentPanelButtons();
            UpdateButtonSelection();
        }
    }

    private void InitializePanels()
    {
        panel.SetActive(false);
        mainPanel.SetActive(false);
        foreach (var subPanel in subPanels)
        {
            subPanel.SetActive(false);
        }
        currentPanelLevel = PanelLevel.Main;
        currentSubPanelIndex = -1;
    }

    // 分别收集各个面板的按钮
    private void CollectPanelButtons()
    {
        mainPanelButtons.Clear();
        subPanelButtons.Clear();
        
        // 收集主面板的按钮
        if (mainPanel != null)
        {
            Button[] buttons = mainPanel.GetComponentsInChildren<Button>(true);
            mainPanelButtons.AddRange(buttons);
        }

        // 收集每个二级面板的按钮
        foreach (var subPanel in subPanels)
        {
            if (subPanel != null && !subPanelButtons.ContainsKey(subPanel))
            {
                Button[] buttons = subPanel.GetComponentsInChildren<Button>(true);
                subPanelButtons[subPanel] = new List<Button>(buttons);
            }
        }

        // 设置当前面板的按钮
        UpdateCurrentPanelButtons();
    }

    // 更新当前面板的按钮列表
    private void UpdateCurrentPanelButtons()
    {
        currentPanelButtons.Clear();
        
        if (currentPanelLevel == PanelLevel.Main)
        {
            currentPanelButtons.AddRange(mainPanelButtons);
        }
        else if (currentPanelLevel == PanelLevel.Sub && currentSubPanelIndex >= 0 && currentSubPanelIndex < subPanels.Length)
        {
            GameObject currentSubPanel = subPanels[currentSubPanelIndex];
            if (subPanelButtons.ContainsKey(currentSubPanel))
            {
                currentPanelButtons.AddRange(subPanelButtons[currentSubPanel]);
            }
        }
        currentButtonIndex = 0;                                                             //重置按钮索引
        UpdateCurrentScrollRect();
        if (currentScrollRect != null) currentScrollRect.verticalNormalizedPosition = 1f;// 将滚动位置设置为顶部 (1 = 顶部, 0 = 底部)
    }

    // 查找所有按钮的TextMeshPro子对象
    private void FindAllButtonTexts()
    {
        buttonTexts.Clear();
        // 遍历主面板按钮
        foreach (var button in mainPanelButtons)
        {
            FindButtonTexts(button);
        }
        
        // 遍历所有二级面板按钮
        foreach (var buttonList in subPanelButtons.Values)
        {
            foreach (var button in buttonList)
            {
                FindButtonTexts(button);
            }
        }
    }

    // 查找单个按钮的TextMeshPro子对象
    private void FindButtonTexts(Button button)
    {
        TextMeshProUGUI[] textComponents = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>(textComponents);
        buttonTexts[button] = texts;
        
        UpdateButtonTextDisplay(button);
    }

    // 更新按钮文本显示
    private void UpdateButtonTextDisplay(Button button)
    {
        if (buttonTexts.ContainsKey(button))
        {
            List<TextMeshProUGUI> texts = buttonTexts[button];
            
            if (texts.Count > 1)
            {
                for (int i = 0; i < texts.Count; i++)
                {
                    if (texts[i] != null)
                    {
                        texts[i].gameObject.SetActive(i == globalOptionIndex);
                    }
                }
            }
            else if (texts.Count == 1 && texts[0] != null)
            {
                texts[0].gameObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        if (playerInputManager != null && playerInputManager.isRebinding) return;// 如果正在重绑定，完全跳过所有输入处理
        
        if (playerInputManager.GetButtonDown("UIMenu"))
        {
            ToggleSystemPanel();
        }
        if (playerInputManager.GetButtonDown("UICancel") && panel.activeSelf)
        {
            if (currentPanelLevel == PanelLevel.Sub)
            {
                ShowMainPanel();            // 如果在二级面板，返回主面板
            }
            else
            {
                ToggleSystemPanel();        // 如果在主面板，切换系统面板显示/隐藏
            }
        }

        if (panel.activeSelf && navigationEnabled)
        {
            HandleKeyboardNavigation();
        }
    }

    // 统一的键盘导航处理
    private void HandleKeyboardNavigation()
    {
        if (playerInputManager.GetButtonDown("UIUp"))
        {
            HandleScroll(currentButtonIndex, true);
            currentButtonIndex--;
            if (currentButtonIndex < 0)
                currentButtonIndex = currentPanelButtons.Count - 1;
            UpdateButtonSelection();
        }
        else if (playerInputManager.GetButtonDown("UIDown"))
        {
            HandleScroll(currentButtonIndex, false);
            currentButtonIndex++;
            if (currentButtonIndex >= currentPanelButtons.Count)
                currentButtonIndex = 0;
            UpdateButtonSelection();
        }
        else if (playerInputManager.GetButtonDown("UILeft"))
        {
            OnOptionChanged?.Invoke(-1);
        }
        else if (playerInputManager.GetButtonDown("UIRight"))
        {
            OnOptionChanged?.Invoke(1);
        }
        else if (playerInputManager.GetButtonDown("UIConfirm"))
        {
            Button currentButton = GetCurrentSelectedButton();
            if (currentButton != null)
                currentButton.onClick.Invoke();
        }
    }
    
    private void HandleScroll(int buttonIndex, bool isUpward)                                     //处理滚动逻辑
    {
        int buttonCount = currentPanelButtons.Count;
        float thresholdBottom = buttonCount - thresholdTop;
        if (currentScrollRect == null || buttonCount == 0) return;
        if (buttonIndex == 0 && isUpward)
        {
            currentScrollRect.verticalNormalizedPosition = 0f;
            return;
        }

        if (buttonIndex == buttonCount - 1 && !isUpward)
        {
            currentScrollRect.verticalNormalizedPosition = 1f;
            return;
        }
        if (isUpward && (buttonIndex < thresholdTop || buttonIndex > thresholdBottom)) return;    //向上滚动：如果新索引在顶部阈值范围内
        if (!isUpward && (buttonIndex < thresholdTop || buttonIndex > thresholdBottom)) return;   //向下滚动：如果新索引在底部阈值范围内
        ScrollContent(isUpward);
    }
    
    private void ScrollContent(bool scrollUp)                                                     //滚动内容
    {
        float currentPosition = currentScrollRect.verticalNormalizedPosition;// 获取当前滚动位置
        
        // 计算滚动步长（基于内容高度）
        RectTransform content = currentScrollRect.content;
        float contentHeight = content.rect.height;
        float viewportHeight = currentScrollRect.viewport.rect.height;
        
        if (contentHeight <= viewportHeight) return; // 内容不足一屏，不需要滚动
        
        float scrollAmount = scrollStep / (contentHeight - viewportHeight);
        
        // 根据方向调整滚动位置
        if (scrollUp)
            currentScrollRect.verticalNormalizedPosition = Mathf.Clamp01(currentPosition + scrollAmount);
        else
            currentScrollRect.verticalNormalizedPosition = Mathf.Clamp01(currentPosition - scrollAmount);
    }
    
    public Button GetCurrentSelectedButton()// 获取当前选中的按钮
    {
        if (currentButtonIndex >= 0 && currentButtonIndex < currentPanelButtons.Count)
        {
            return currentPanelButtons[currentButtonIndex];
        }
        return null;
    }

    // 更新按钮选择状态
    private void UpdateButtonSelection()
    {
        // 重置所有按钮颜色
        ResetCurrentPanelButtonColors();

        // 设置当前选中按钮的颜色
        Button currentButton = GetCurrentSelectedButton();
        if (currentButton != null && currentButton.interactable)
        {
            var colors = currentButton.colors;
            colors.normalColor = selectedColor;
            colors.selectedColor = selectedColor;
            currentButton.colors = colors;
        }
    }

    // 重置当前面板所有按钮颜色
    private void ResetCurrentPanelButtonColors()
    {
        foreach (var button in currentPanelButtons)
        {
            var colors = button.colors;
            colors.normalColor = normalColor;
            colors.selectedColor = normalColor;
            button.colors = colors;
        }
    }

    // 切换系统面板的显示/隐藏
    public void ToggleSystemPanel()
    {
        bool isActive = !panel.activeSelf;
        panel.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;
        
        if (isActive)
        {
            ShowMainPanel();
            UpdateButtonSelection();
        }
        else
        {
            currentPanelLevel = PanelLevel.Main;                               //关闭面板时重置状态
            currentSubPanelIndex = -1;
            ResetCurrentPanelButtonColors();
        }
    }
    
    public void ShowMainPanel()                                                 //显示主面板
    {
        GetComponent<Canvas>().sortingOrder = 100;                              //提高渲染层级
        foreach (var subPanel in subPanels)                          //隐藏所有二级面板
        {
            subPanel.SetActive(false);
        }
        mainPanel.SetActive(true);                                              //显示主面板
        currentPanelLevel = PanelLevel.Main;
        currentSubPanelIndex = -1;
        UpdateCurrentPanelButtons();
        UpdateButtonSelection();
    }
    
    public void EnterSubPanel(int subPanelIndex)                                //进入二级面板
    {
        mainPanel.SetActive(false);                                             //隐藏主面板
        subPanels[subPanelIndex].SetActive(true);                               //显示指定的二级面板
        currentSubPanelIndex = subPanelIndex;
        currentPanelLevel = PanelLevel.Sub;
        UpdateCurrentPanelButtons();
        UpdateButtonSelection();
    }
}