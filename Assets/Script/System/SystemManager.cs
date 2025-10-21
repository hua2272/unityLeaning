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
    
    // 按钮管理
    private List<Button> mainPanelButtons = new List<Button>();
    private List<List<Button>> subPanelButtons = new List<List<Button>>();
    private int currentMainButtonIndex = 0;
    private int currentSubButtonIndex = 0;
    
    private int currentSubPanelIndex = -1;                                            // -1表示在主面板
    private bool isInSubPanel = false;
    private bool navigationEnabled = true;

    // 修改：选项按钮管理 - 存储按钮对应的TextMeshPro选项列表和当前选项索引
    private Dictionary<Button, List<TextMeshProUGUI>> optionButtons = new Dictionary<Button, List<TextMeshProUGUI>>();
    private Dictionary<Button, int> optionCurrentIndex = new Dictionary<Button, int>();

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
        FindAllOptionButtons();
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

    // 修改：查找所有选项按钮 - 现在查找带有多个TextMeshPro子对象的按钮
    private void FindAllOptionButtons()
    {
        optionButtons.Clear();
        optionCurrentIndex.Clear();
        
        // 查找主面板中的选项按钮
        if (mainPanel != null)
        {
            foreach (var button in mainPanelButtons)
            {
                if (button != null)
                {
                    FindOptionButtonInButton(button);
                }
            }
        }

        // 查找二级面板中的选项按钮
        foreach (var buttonList in subPanelButtons)
        {
            foreach (var button in buttonList)
            {
                if (button != null)
                {
                    FindOptionButtonInButton(button);
                }
            }
        }
    }

    // 新增：在单个按钮中查找选项
    private void FindOptionButtonInButton(Button button)
    {
        // 获取按钮下所有的TextMeshProUGUI组件
        TextMeshProUGUI[] textComponents = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        
        // 如果有多个TextMeshPro子对象，则认为是选项按钮
        if (textComponents.Length > 1)
        {
            List<TextMeshProUGUI> optionTexts = new List<TextMeshProUGUI>(textComponents);
            optionButtons[button] = optionTexts;
            optionCurrentIndex[button] = 0; // 默认选择第一个选项
            
            // 初始化选项显示状态
            UpdateOptionButtonDisplay(button);
        }
    }

    // 新增：更新选项按钮的显示状态
    private void UpdateOptionButtonDisplay(Button button)
    {
        if (optionButtons.ContainsKey(button))
        {
            List<TextMeshProUGUI> options = optionButtons[button];
            int currentIndex = optionCurrentIndex[button];
            
            // 激活当前选中的选项，禁用其他选项
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] != null)
                {
                    options[i].gameObject.SetActive(i == currentIndex);
                }
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
            // 向左选择选项（仅对选项按钮有效）
            HandleLeftOption();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // 向右选择选项（仅对选项按钮有效）
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
            // 向左选择选项（仅对选项按钮有效）
            HandleLeftOption();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // 向右选择选项（仅对选项按钮有效）
            HandleRightOption();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            // 触发当前选中的按钮
            TriggerCurrentSubButton();
        }
    }

    // 修改：处理向左选择选项 - 现在循环切换TextMeshPro子对象
    private void HandleLeftOption()
    {
        Button currentButton = GetCurrentSelectedButton();
        if (currentButton != null && optionButtons.ContainsKey(currentButton))
        {
            List<TextMeshProUGUI> options = optionButtons[currentButton];
            int currentIndex = optionCurrentIndex[currentButton];
            
            // 计算新的索引（循环）
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = options.Count - 1;
            
            optionCurrentIndex[currentButton] = currentIndex;
            UpdateOptionButtonDisplay(currentButton);
        }
    }

    // 修改：处理向右选择选项 - 现在循环切换TextMeshPro子对象
    private void HandleRightOption()
    {
        Button currentButton = GetCurrentSelectedButton();
        if (currentButton != null && optionButtons.ContainsKey(currentButton))
        {
            List<TextMeshProUGUI> options = optionButtons[currentButton];
            int currentIndex = optionCurrentIndex[currentButton];
            
            // 计算新的索引（循环）
            currentIndex++;
            if (currentIndex >= options.Count)
                currentIndex = 0;
            
            optionCurrentIndex[currentButton] = currentIndex;
            UpdateOptionButtonDisplay(currentButton);
        }
    }

    // 新增：获取当前选中的按钮
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

    // 触发当前主面板按钮
    private void TriggerCurrentMainButton()
    {
        if (currentMainButtonIndex >= 0 && currentMainButtonIndex < mainPanelButtons.Count)
        {
            var button = mainPanelButtons[currentMainButtonIndex];
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
        }
    }

    // 触发当前二级面板按钮
    private void TriggerCurrentSubButton()
    {
        if (currentSubPanelIndex >= 0 && currentSubPanelIndex < subPanelButtons.Count && 
            currentSubButtonIndex >= 0 && currentSubButtonIndex < subPanelButtons[currentSubPanelIndex].Count)
        {
            var button = subPanelButtons[currentSubPanelIndex][currentSubButtonIndex];
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
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

    // 新增：获取选项按钮的当前选项索引
    public int GetOptionCurrentIndex(Button button)
    {
        if (optionCurrentIndex.ContainsKey(button))
        {
            return optionCurrentIndex[button];
        }
        return -1;
    }

    // 新增：设置选项按钮的当前选项索引
    public void SetOptionCurrentIndex(Button button, int index)
    {
        if (optionButtons.ContainsKey(button) && index >= 0 && index < optionButtons[button].Count)
        {
            optionCurrentIndex[button] = index;
            UpdateOptionButtonDisplay(button);
        }
    }
}