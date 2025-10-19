using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InputRebindingUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject waitingForInputPanel;
    public TextMeshProUGUI waitingForInputText;
    public Button resetToDefaultsButton;
    public Button confirmButton;
    public Button cancelButton;
    
    [Header("Action Button Bindings")]
    public Button moveUpButton;
    public Button moveDownButton;
    public Button moveLeftButton;
    public Button moveRightButton;
    public Button jumpButton;
    // public Button attackButton;
    // public Button interactButton;
    // public Button menuButton;
    
    private Dictionary<string, Button> actionButtons = new Dictionary<string, Button>();
    private CustomInputSystem inputSystem;

    void Start()
    {
        inputSystem = CustomInputSystem.instance;
        InitializeButtonBindings();
        SetupEventListeners();
        
        waitingForInputPanel.SetActive(false);
    }

    void InitializeButtonBindings()
    {
        // 手动绑定每个按钮到对应的动作
        actionButtons["Jump"] = jumpButton;
        actionButtons["MoveUp"] = moveUpButton;
        actionButtons["MoveDown"] = moveDownButton;
        actionButtons["MoveLeft"] = moveLeftButton;
        actionButtons["MoveRight"] = moveRightButton;
        // actionButtons["Menu"] = menuButton;
        // actionButtons["Attack"] = attackButton;
        // actionButtons["Interact"] = interactButton;
        
        // 初始化所有按钮文本
        UpdateAllButtonTexts();
    }

    void SetupEventListeners()
    {
        // 为每个动作按钮添加点击事件
        jumpButton.onClick.AddListener(() => StartRebinding("Jump", true));
        moveUpButton.onClick.AddListener(() => StartRebinding("MoveUp", true));
        moveDownButton.onClick.AddListener(() => StartRebinding("MoveDown", true));
        moveLeftButton.onClick.AddListener(() => StartRebinding("MoveLeft", true));
        moveRightButton.onClick.AddListener(() => StartRebinding("MoveRight", true));
        // menuButton.onClick.AddListener(() => StartRebinding("Menu", true));
        // attackButton.onClick.AddListener(() => StartRebinding("Attack", true));
        // interactButton.onClick.AddListener(() => StartRebinding("Interact", true));
        
        // 其他功能按钮
        resetToDefaultsButton.onClick.AddListener(ResetToDefaults);
        confirmButton.onClick.AddListener(ConfirmChanges);
        cancelButton.onClick.AddListener(CancelChanges);
        
        // 注册输入系统事件
        inputSystem.OnActionRebound += OnActionRebound;
    }

    void StartRebinding(string actionName, bool forKeyboard)
    {
        if (inputSystem.isRebinding) return;
        
        // 显示等待输入提示
        waitingForInputPanel.SetActive(true);
        waitingForInputText.text = $"等待输入...\n<size=70%>为 {GetDisplayName(actionName)} 绑定按键</size>";
        
        // 禁用所有按钮避免重复点击
        SetAllButtonsInteractable(false);
        
        // 开始重绑定
        inputSystem.StartRebinding(actionName, forKeyboard);
        
        // 开始检测重绑定完成
        StartCoroutine(WaitForRebindingComplete());
    }

    IEnumerator WaitForRebindingComplete()
    {
        while (inputSystem.isRebinding)
        {
            yield return null;
        }
        
        // 重绑定完成，更新UI
        waitingForInputPanel.SetActive(false);
        SetAllButtonsInteractable(true);
    }

    void OnActionRebound(string actionName)
    {
        // 更新特定按钮的文本
        if (actionButtons.ContainsKey(actionName))
        {
            UpdateButtonText(actionButtons[actionName], actionName);
        }
    }

    void UpdateButtonText(Button button, string actionName)
    {
        var action = inputSystem.inputActions.Find(a => a.actionName == actionName);
        if (action != null)
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = GetKeyDisplayName(action.currentKeyboardKey);
        }
    }

    void UpdateAllButtonTexts()
    {
        foreach (var kvp in actionButtons)
        {
            UpdateButtonText(kvp.Value, kvp.Key);
        }
    }

    void SetAllButtonsInteractable(bool interactable)
    {
        foreach (var button in actionButtons.Values)
        {
            button.interactable = interactable;
        }
        resetToDefaultsButton.interactable = interactable;
        confirmButton.interactable = interactable;
        cancelButton.interactable = interactable;
    }

    void ResetToDefaults()
    {
        inputSystem.ResetToDefaults();
        UpdateAllButtonTexts();
    }

    void ConfirmChanges()
    {
        inputSystem.SaveKeyBindings();
        // 可以添加保存成功的反馈
        Debug.Log("按键设置已保存");
    }

    void CancelChanges()
    {
        // 重新加载已保存的设置
        inputSystem.LoadKeyBindings();
        UpdateAllButtonTexts();
        Debug.Log("已取消更改");
    }

    #region 显示名称转换（同上）
    string GetDisplayName(string actionName)
    {
        return actionName switch
        {
            "MoveUp" => "向上移动",
            "MoveDown" => "向下移动", 
            "MoveLeft" => "向左移动",
            "MoveRight" => "向右移动",
            "Jump" => "跳跃",
            "Attack" => "攻击",
            "Interact" => "交互",
            "Menu" => "菜单",
            _ => actionName
        };
    }

    string GetKeyDisplayName(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.Mouse0 => "鼠标左键",
            KeyCode.Mouse1 => "鼠标右键",
            KeyCode.Mouse2 => "鼠标中键",
            KeyCode.UpArrow => "上箭头",
            KeyCode.DownArrow => "下箭头", 
            KeyCode.LeftArrow => "左箭头",
            KeyCode.RightArrow => "右箭头",
            KeyCode.Return => "回车",
            KeyCode.Escape => "ESC",
            KeyCode.Space => "空格",
            KeyCode.LeftShift => "左Shift",
            KeyCode.RightShift => "右Shift",
            KeyCode.LeftControl => "左Ctrl",
            KeyCode.RightControl => "右Ctrl",
            KeyCode.LeftAlt => "左Alt",
            KeyCode.RightAlt => "右Alt",
            _ => keyCode.ToString()
        };
    }
    #endregion

    void OnDestroy()
    {
        if (inputSystem != null)
        {
            inputSystem.OnActionRebound -= OnActionRebound;
        }
    }
}