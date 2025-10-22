using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SystemManager : MonoBehaviour
{
    public static SystemManager instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject panel;                                       // 总面板
    [SerializeField] private GameObject mainPanel;                                   // 主要面板
    [SerializeField] private GameObject[] subPanels;                                 // 二级面板
                    
    [Header("Navigation Settings")]
    [SerializeField] private int defaultSubPanelIndex = 0;                           // 默认二级面板索引
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    
    [Header("Global Option Parameter")]
    [SerializeField] private int globalOptionIndex = 0;                              // 全局选项索引
    
    // 按钮管理
    private List<Button> mainPanelButtons = new List<Button>();
    private List<List<Button>> subPanelButtons = new List<List<Button>>();
    private int currentMainButtonIndex = 0;
    private int currentSubButtonIndex = 0;
    
    private int currentSubPanelIndex = -1;                                            // -1表示在主面板
    private bool isInSubPanel = false;
    private bool navigationEnabled = true;

    // 简化：只存储每个按钮的TextMeshPro子对象列表
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

    private void CollectAllButtons()
    {
        // 收集主面板的所有按钮
        mainPanelButtons.Clear();
        if (mainPanel != null)
        {
            Button[] buttons = mainPanel.GetComponentsInChildren<Button>(true);
            mainPanelButtons.AddRange(buttons);
        }

        // 收集每个二级面板的所有按钮
        subPanelButtons.Clear();
        foreach (var subPanel in subPanels)
        {
            if (subPanel != null)
            {
                Button[] buttons = subPanel.GetComponentsInChildren<Button>(true);
                subPanelButtons.Add(new List<Button>(buttons));
            }
            else
            {
                subPanelButtons.Add(new List<Button>());
            }
        }
    }

    // 简化：查找所有按钮的TextMeshPro子对象
    private void FindAllButtonTexts()
    {
        buttonTexts.Clear();
        
        // 处理主面板按钮
        foreach (var button in mainPanelButtons)
        {
            if (button != null)
            {
                FindButtonTexts(button);
            }
        }

        // 处理二级面板按钮
        foreach (var buttonList in subPanelButtons)
        {
            foreach (var button in buttonList)
            {
                if (button != null)
                {
                    FindButtonTexts(button);
                }
            }
        }
    }

    // 查找单个按钮的TextMeshPro子对象
    private void FindButtonTexts(Button button)
    {
        // 获取按钮下所有的TextMeshProUGUI组件
        TextMeshProUGUI[] textComponents = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        
        // 存储所有TextMeshPro子对象
        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>(textComponents);
        buttonTexts[button] = texts;
        
        // 更新按钮显示
        UpdateButtonTextDisplay(button);
    }

    // 更新按钮文本显示
    private void UpdateButtonTextDisplay(Button button)
    {
        if (buttonTexts.ContainsKey(button))
        {
            List<TextMeshProUGUI> texts = buttonTexts[button];
            
            // 如果有多个文本对象，只显示与全局选项索引对应的那个
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
            // 如果只有一个文本对象，始终显示它
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
        if(globalOptionIndex == 0){Debug.Log("globalOptionIndex 0");}
        if(globalOptionIndex == 1){Debug.Log("----------1");}
    }

    // 处理键盘导航
    private void HandleKeyboardNavigation()
    {
        if (isInSubPanel)
        {
            // 在二级面板中导航
            HandleSubPanelNavigation();
        }
        else
        {
            // 在主面板中导航
            HandleMainPanelNavigation();
        }
    }

    // 处理主面板导航
    private void HandleMainPanelNavigation()
    {
        if (mainPanelButtons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            // 向上选择
            currentMainButtonIndex--;
            if (currentMainButtonIndex < 0)
                currentMainButtonIndex = mainPanelButtons.Count - 1;
            
            UpdateButtonSelection();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // 向下选择
            currentMainButtonIndex++;
            if (currentMainButtonIndex >= mainPanelButtons.Count)
                currentMainButtonIndex = 0;
            
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
        else if (Input.GetKeyDown(KeyCode.J))
        {
            // 触发当前选中的按钮
            TriggerCurrentMainButton();
        }
    }

    // 处理二级面板导航
    private void HandleSubPanelNavigation()
    {
        if (currentSubPanelIndex < 0 || currentSubPanelIndex >= subPanelButtons.Count) return;
        
        var currentSubButtons = subPanelButtons[currentSubPanelIndex];
        if (currentSubButtons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            // 向上选择
            currentSubButtonIndex--;
            if (currentSubButtonIndex < 0)
                currentSubButtonIndex = currentSubButtons.Count - 1;
            
            UpdateButtonSelection();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // 向下选择
            currentSubButtonIndex++;
            if (currentSubButtonIndex >= currentSubButtons.Count)
                currentSubButtonIndex = 0;
            
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
        else if (Input.GetKeyDown(KeyCode.J))
        {
            // 触发当前选中的按钮
            TriggerCurrentSubButton();
        }
    }

    // 简化：处理向左选择选项 - 更新全局选项索引
    private void HandleLeftOption()
    {
        // 减少全局选项索引
        globalOptionIndex--;
        
        // 循环索引
        if (globalOptionIndex < 0)
        {
            // 找到最大索引值
            int maxIndex = 0;
            foreach (var texts in buttonTexts.Values)
            {
                if (texts.Count > maxIndex)
                    maxIndex = texts.Count;
            }
            
            globalOptionIndex = maxIndex > 0 ? maxIndex - 1 : 0;
        }
        
        // 更新所有按钮的显示
        UpdateAllButtonTexts();
    }

    // 简化：处理向右选择选项 - 更新全局选项索引
    private void HandleRightOption()
    {
        // 增加全局选项索引
        globalOptionIndex++;
        
        // 找到最大索引值
        int maxIndex = 0;
        foreach (var texts in buttonTexts.Values)
        {
            if (texts.Count > maxIndex)
                maxIndex = texts.Count;
        }
        
        // 循环索引
        if (maxIndex > 0 && globalOptionIndex >= maxIndex)
        {
            globalOptionIndex = 0;
        }
        
        // 更新所有按钮的显示
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
        if (isInSubPanel)
        {
            if (currentSubPanelIndex >= 0 && currentSubPanelIndex < subPanelButtons.Count && 
                currentSubButtonIndex >= 0 && currentSubButtonIndex < subPanelButtons[currentSubPanelIndex].Count)
            {
                return subPanelButtons[currentSubPanelIndex][currentSubButtonIndex];
            }
        }
        else
        {
            if (currentMainButtonIndex >= 0 && currentMainButtonIndex < mainPanelButtons.Count)
            {
                return mainPanelButtons[currentMainButtonIndex];
            }
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
        // 重置主面板按钮
        foreach (var button in mainPanelButtons)
        {
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = normalColor;
                colors.selectedColor = normalColor;
                button.colors = colors;
            }
        }

        // 重置所有二级面板按钮
        foreach (var buttonList in subPanelButtons)
        {
            foreach (var button in buttonList)
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
    }

    // 简化：触发当前主面板按钮
    private void TriggerCurrentMainButton()
    {
        if (currentMainButtonIndex >= 0 && currentMainButtonIndex < mainPanelButtons.Count)
        {
            var button = mainPanelButtons[currentMainButtonIndex];
            if (button != null && button.interactable)
            {
                // 触发按钮点击事件，传递全局选项索引
                TriggerButton(button);
            }
        }
    }

    // 简化：触发当前二级面板按钮
    private void TriggerCurrentSubButton()
    {
        if (currentSubPanelIndex >= 0 && currentSubPanelIndex < subPanelButtons.Count && 
            currentSubButtonIndex >= 0 && currentSubButtonIndex < subPanelButtons[currentSubPanelIndex].Count)
        {
            var button = subPanelButtons[currentSubPanelIndex][currentSubButtonIndex];
            if (button != null && button.interactable)
            {
                // 触发按钮点击事件，传递全局选项索引
                TriggerButton(button);
            }
        }
    }

    // 简化：触发按钮
    private void TriggerButton(Button button)
    {
        // 直接触发按钮的点击事件
        button.onClick.Invoke();
        
        // 可以通过全局选项索引获取当前选中的选项
        // 其他脚本可以通过SystemManager.instance.GetGlobalOptionIndex()获取当前全局选项索引
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
        
        // 如果打开面板，显示主面板并重置导航状态
        if (isActive)
        {
            ShowMainPanel();
            currentMainButtonIndex = 0;
            UpdateButtonSelection();
        }
        else
        {
            // 关闭面板时重置状态
            isInSubPanel = false;
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
            
        isInSubPanel = false;
        currentSubPanelIndex = -1;
        currentMainButtonIndex = 0;
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
            isInSubPanel = true;
            currentSubButtonIndex = 0;
            UpdateButtonSelection();
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
        ResetAllButtonColors();
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

    // 启用/禁用导航（可用于临时禁用键盘导航）
    public void SetNavigationEnabled(bool enabled)
    {
        navigationEnabled = enabled;
        if (!enabled)
        {
            ResetAllButtonColors();
        }
    }

    // 新增：获取全局选项索引
    public int GetGlobalOptionIndex()
    {
        return globalOptionIndex;
    }

    // 新增：设置全局选项索引
    public void SetGlobalOptionIndex(int index)
    {
        globalOptionIndex = index;
        
        // 确保索引在有效范围内
        int maxIndex = 0;
        foreach (var texts in buttonTexts.Values)
        {
            if (texts.Count > maxIndex)
                maxIndex = texts.Count;
        }
        
        if (maxIndex > 0 && globalOptionIndex >= maxIndex)
        {
            globalOptionIndex = maxIndex - 1;
        }
        else if (globalOptionIndex < 0)
        {
            globalOptionIndex = 0;
        }
        
        // 更新所有按钮的显示
        UpdateAllButtonTexts();
    }

    // 新增：获取按钮的文本对象列表
    public List<TextMeshProUGUI> GetButtonTexts(Button button)
    {
        if (buttonTexts.ContainsKey(button))
        {
            return buttonTexts[button];
        }
        return null;
    }
}