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
    
    // 统一的按钮管理
    private List<Button> allButtons = new List<Button>();
    private int currentButtonIndex = 0;
    
    // 面板层级管理
    private enum PanelLevel { Main, Sub }
    private PanelLevel currentPanelLevel = PanelLevel.Main;
    private int currentSubPanelIndex = -1;
    
    private bool navigationEnabled = true;
    private bool isInRebindingProcess = false; // 新增：标记是否在重绑定过程中

    // 按钮文本管理
    private Dictionary<Button, List<TextMeshProUGUI>> buttonTexts = new Dictionary<Button, List<TextMeshProUGUI>>();

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
        CollectAllButtons();
        FindAllButtonTexts();
    }

    private void Start()
    {
        playerInputManager = PlayerInputManager.instance;
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
        currentPanelLevel = PanelLevel.Main;
        currentSubPanelIndex = -1;
    }

    private void CollectAllButtons()
    {
        allButtons.Clear();
        
        // 收集主面板的所有按钮
        if (mainPanel != null)
        {
            Button[] buttons = mainPanel.GetComponentsInChildren<Button>(true);
            allButtons.AddRange(buttons);
        }

        // 收集所有二级面板的所有按钮
        foreach (var subPanel in subPanels)
        {
            if (subPanel != null)
            {
                Button[] buttons = subPanel.GetComponentsInChildren<Button>(true);
                allButtons.AddRange(buttons);
            }
        }
    }

    // 查找所有按钮的TextMeshPro子对象
    private void FindAllButtonTexts()
    {
        buttonTexts.Clear();
        
        foreach (var button in allButtons)
        {
            if (button != null)
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }

        if (panel.activeSelf && navigationEnabled)
        {
            HandleKeyboardNavigation();
        }
    }

    // 统一的键盘导航处理
    private void HandleKeyboardNavigation()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            // 向上选择
            currentButtonIndex--;
            if (currentButtonIndex < 0)
                currentButtonIndex = allButtons.Count - 1;
            
            UpdateButtonSelection();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // 向下选择
            currentButtonIndex++;
            if (currentButtonIndex >= allButtons.Count)
                currentButtonIndex = 0;
            
            UpdateButtonSelection();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            // 向左选择选项
            HandleLeftOption();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // 向右选择选项
            HandleRightOption();
        }
        else if (playerInputManager.GetButton("UIConfirm"))
        {
            // 触发当前选中的按钮
            TriggerCurrentButton();
        }
    }

    // 处理向左选择选项
    private void HandleLeftOption()
    {
        globalOptionIndex--;
        
        if (globalOptionIndex < 0)
        {
            int maxIndex = 0;
            foreach (var texts in buttonTexts.Values)
            {
                if (texts.Count > maxIndex)
                    maxIndex = texts.Count;
            }
            
            globalOptionIndex = maxIndex > 0 ? maxIndex - 1 : 0;
        }
        
        UpdateAllButtonTexts();
    }

    // 处理向右选择选项
    private void HandleRightOption()
    {
        globalOptionIndex++;
        
        int maxIndex = 0;
        foreach (var texts in buttonTexts.Values)
        {
            if (texts.Count > maxIndex)
                maxIndex = texts.Count;
        }
        
        if (maxIndex > 0 && globalOptionIndex >= maxIndex)
        {
            globalOptionIndex = 0;
        }
        
        UpdateAllButtonTexts();
    }

    // 更新所有按钮的文本显示
    private void UpdateAllButtonTexts()
    {
        foreach (var button in buttonTexts.Keys)
        {
            UpdateButtonTextDisplay(button);
        }
    }

    // 获取当前选中的按钮
    private Button GetCurrentSelectedButton()
    {
        if (currentButtonIndex >= 0 && currentButtonIndex < allButtons.Count)
        {
            return allButtons[currentButtonIndex];
        }
        return null;
    }

    // 更新按钮选择状态
    private void UpdateButtonSelection()
    {
        // 重置所有按钮颜色
        ResetAllButtonColors();

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

    // 重置所有按钮颜色
    private void ResetAllButtonColors()
    {
        foreach (var button in allButtons)
        {
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = normalColor;
                colors.selectedColor = normalColor;
                button.colors = colors;
            }
        }
    }

    // 触发当前按钮
    private void TriggerCurrentButton()
    {
        Button currentButton = GetCurrentSelectedButton();
        if (currentButton.name.Equals("confirm"))
        {
            Debug.LogWarning("----------rebind-----------");
            playerInputManager.StartRebinding("UIConfirm");
            return;
        }
        Debug.Log("----------" + currentButton.name + "-----------");
        Debug.Log("----------" + currentButtonIndex + "-----------");
        currentButton.onClick.Invoke();
    }

    // 处理ESC键逻辑
    private void HandleEscapeKey()
    {
        if (currentPanelLevel == PanelLevel.Sub)
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
        
        if (isActive)
        {
            ShowMainPanel();
            currentButtonIndex = 0;
            UpdateButtonSelection();
        }
        else
        {
            // 关闭面板时重置状态
            currentPanelLevel = PanelLevel.Main;
            currentSubPanelIndex = -1;
            ResetAllButtonColors();
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
            
        currentPanelLevel = PanelLevel.Main;
        currentSubPanelIndex = -1;
        currentButtonIndex = 0;
        UpdateButtonSelection();
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
            currentPanelLevel = PanelLevel.Sub;
            currentButtonIndex = 0;
            UpdateButtonSelection();
        }
    }

    // 返回到主面板
    public void ReturnToMainPanel()
    {
        ShowMainPanel();
    }
}