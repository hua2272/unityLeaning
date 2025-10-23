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
    
    [Header("Action Button Bindings")]
    public Button moveUpButton;
    public Button moveDownButton;
    public Button moveLeftButton;
    public Button moveRightButton;
    public Button jumpButton;
    // public Button attackButton;
    // public Button interactButton;
    // public Button menuButton;
    
    [Header("System Button Bindings")]
    // public Button up;
    // public Button down;
    // public Button left;
    // public Button right;
    public Button confirm;    // 用于UI确认的按键绑定
    public Button cancel;     // 用于UI取消的按键绑定
    
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
        actionButtons["UIConfirm"] = confirm;
        actionButtons["UICancel"] = cancel;
        // actionButtons["Menu"] = menuButton;
        // actionButtons["Attack"] = attackButton;
        // actionButtons["Interact"] = interactButton;
        
        // 初始化所有按钮文本
        foreach (var kvp in actionButtons)
        {
            UpdateButtonText(kvp.Value, kvp.Key);
        }
    }

    void SetupEventListeners()
    {
        // 为每个动作按钮添加点击事件
        jumpButton.onClick.AddListener(() => StartRebinding("Jump", true));
        moveUpButton.onClick.AddListener(() => StartRebinding("MoveUp", true));
        moveDownButton.onClick.AddListener(() => StartRebinding("MoveDown", true));
        moveLeftButton.onClick.AddListener(() => StartRebinding("MoveLeft", true));
        moveRightButton.onClick.AddListener(() => StartRebinding("MoveRight", true));
        confirm.onClick.AddListener(() => StartRebinding("UIConfirm", true));
        cancel.onClick.AddListener(() => StartRebinding("UICancel", true));
        // menuButton.onClick.AddListener(() => StartRebinding("Menu", true));
        // attackButton.onClick.AddListener(() => StartRebinding("Attack", true));
        // interactButton.onClick.AddListener(() => StartRebinding("Interact", true));
        resetToDefaultsButton.onClick.AddListener(ResetToDefaults);
        
        inputSystem.OnActionRebound.AddListener(OnActionRebound);       //注册输入系统事件，绑定按键更新UI
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
        
        // 即时保存按键设置
        inputSystem.SaveKeyBindings();
        Debug.Log($"按键设置已即时保存");
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

    void SetAllButtonsInteractable(bool interactable)
    {
        foreach (var button in actionButtons.Values)
        {
            button.interactable = interactable;
        }
        resetToDefaultsButton.interactable = interactable;
    }

    void ResetToDefaults()
    {
        inputSystem.ResetToDefaults();
        foreach (var kvp in actionButtons)
        {
            UpdateButtonText(kvp.Value, kvp.Key);
        }
    }

    #region 显示名称转换
    string GetDisplayName(string actionName)
    {
        return actionName switch
        {
            "MoveUp" => "向上移动",
            "MoveDown" => "向下移动", 
            "MoveLeft" => "向左移动",
            "MoveRight" => "向右移动",
            "Jump" => "跳跃",
            "UIConfirm" => "确认",      // 新增确认键显示名称
            "UICancel" => "取消",       // 新增取消键显示名称
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
            inputSystem.OnActionRebound.RemoveListener(OnActionRebound);
        }
    }
}