using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using System.Collections.Generic;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance { get; private set; }
    
    private bool inputBufferEnabled = false;
    private float inputBufferTime = 0.2f;
    private float lastRebindTime = 0f;
    
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
    public Button attack1Button;
    public Button interactButton;
    public Button menuButton;
    
    [Header("System Button Bindings")]
    public Button confirm;
    public Button cancel;
    
    [System.Serializable]
    public class InputAction
    {
        public string actionName;
        public KeyCode defaultKeyboardKey;
        public KeyCode currentKeyboardKey;
        
        public InputAction(string name, KeyCode keyboardKey)
        {
            actionName = name;
            defaultKeyboardKey = keyboardKey;
            currentKeyboardKey = keyboardKey;
        }
    }
    
    [Header("Input Actions")]
    public List<InputAction> inputActions = new List<InputAction>();
    
    private Dictionary<string, Button> actionButtons = new Dictionary<string, Button>();
    private Dictionary<string, InputAction> actionMap = new Dictionary<string, InputAction>();
    public bool isRebinding = false;
    private string rebindingAction;
    private System.IDisposable currentRebindingOperation;

    void Awake()
    {
        Debug.unityLogger.Log("-------PlayerInputManager instance-------");
        if (instance == null)
        {
            instance = this;
            InitializeInputSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 输入缓冲方法
    private void EnableInputBuffer()
    {
        inputBufferEnabled = true;
        lastRebindTime = Time.unscaledTime;
    }
    
    private void Update()
    {
        // 更新输入缓冲
        if (inputBufferEnabled && Time.unscaledTime - lastRebindTime > inputBufferTime)
        {
            inputBufferEnabled = false;
        }
    }

    void Start()
    {
        InitializeButtonBindings();
        SetupEventListeners();
        
        waitingForInputPanel.SetActive(false);
    }

    void InitializeInputSystem()
    {
        // 创建默认输入映射
        if (inputActions.Count == 0)
        {
            inputActions.Add(new InputAction("MoveUp", KeyCode.W));
            inputActions.Add(new InputAction("MoveDown", KeyCode.S));
            inputActions.Add(new InputAction("MoveLeft", KeyCode.A));
            inputActions.Add(new InputAction("MoveRight", KeyCode.D));
            inputActions.Add(new InputAction("Jump", KeyCode.Space));
            inputActions.Add(new InputAction("Attack1", KeyCode.J));
            inputActions.Add(new InputAction("Interact", KeyCode.E));
            inputActions.Add(new InputAction("Menu", KeyCode.Escape));
            inputActions.Add(new InputAction("UIConfirm", KeyCode.Return));
            inputActions.Add(new InputAction("UICancel", KeyCode.Escape));
        }
        
        // 构建快速查找字典
        foreach (var action in inputActions)
        {
            actionMap[action.actionName] = action;
        }
        
        // 加载保存的按键设置
        LoadKeyBindings();
    }

    void InitializeButtonBindings()
    {
        // 手动绑定每个按钮到对应的动作
        actionButtons["MoveUp"] = moveUpButton;
        actionButtons["MoveDown"] = moveDownButton;
        actionButtons["MoveLeft"] = moveLeftButton;
        actionButtons["MoveRight"] = moveRightButton;
        actionButtons["Jump"] = jumpButton;
        actionButtons["Attack1"] = attack1Button;
        actionButtons["Interact"] = interactButton;
        actionButtons["Menu"] = menuButton;
        actionButtons["UIConfirm"] = confirm;
        actionButtons["UICancel"] = cancel;
        
        // 初始化所有按钮文本
        foreach (var kvp in actionButtons)
        {
            UpdateButtonText(kvp.Value, kvp.Key);
        }
    }

    void SetupEventListeners()
    {
        // 为每个动作按钮添加点击事件
        moveUpButton.onClick.AddListener(() => StartRebinding("MoveUp"));
        moveDownButton.onClick.AddListener(() => StartRebinding("MoveDown"));
        moveLeftButton.onClick.AddListener(() => StartRebinding("MoveLeft"));
        moveRightButton.onClick.AddListener(() => StartRebinding("MoveRight"));
        jumpButton.onClick.AddListener(() => StartRebinding("Jump"));
        attack1Button.onClick.AddListener(() => StartRebinding("Attack1"));
        interactButton.onClick.AddListener(() => StartRebinding("Interact"));
        menuButton.onClick.AddListener(() => StartRebinding("Menu"));
        
        confirm.onClick.AddListener(() => StartRebinding("UIConfirm"));
        cancel.onClick.AddListener(() => StartRebinding("UICancel"));
        
        resetToDefaultsButton.onClick.AddListener(ResetToDefaults);
    }

    public void StartRebinding(string actionName)
    {
        if (isRebinding) return;
        
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return;
        }
        
        waitingForInputPanel.SetActive(true);
        waitingForInputText.text = $"wait for...<size=70%> {actionName} rebinding</size>";
        
        SetAllButtonsInteractable(false); // 禁用所有按钮避免重复点击
        
        isRebinding = true;
        rebindingAction = actionName;
        
        Debug.Log($"Press any key to bind for {actionName}... (Press Escape to cancel)");
        
        // 使用新输入系统的事件监听
        currentRebindingOperation = InputSystem.onAnyButtonPress.CallOnce(OnAnyButtonPressed);
    }

    private void OnAnyButtonPressed(InputControl control)
    {
        if (control is KeyControl keyControl)                                   // 只处理键盘按键
        {
            KeyCode newKeyCode = ConvertToKeyCode(keyControl.keyCode);          // 将新输入系统的 Key 转换为传统的 KeyCode
            if (newKeyCode == KeyCode.None || newKeyCode == KeyCode.Escape)     // 跳过不允许绑定的键
            {
                CancelRebinding();
                return;
            }
            BindKey(rebindingAction, newKeyCode);
        }
    }

    public void CancelRebinding()
    {
        isRebinding = false;
        rebindingAction = null;
        currentRebindingOperation?.Dispose();
        currentRebindingOperation = null;
        Debug.Log("Rebinding cancelled");
        
        waitingForInputPanel.SetActive(false);
        SetAllButtonsInteractable(true);
    }

    void BindKey(string actionName, KeyCode newKey)
    {
        if (!actionMap.ContainsKey(actionName)) return;
        
        // 检查按键是否已被使用
        // foreach (var action in inputActions)
        // {
        //     if (action.actionName != actionName && action.currentKeyboardKey == newKey)
        //     {
        //         Debug.LogWarning($"Key {newKey} is already bound to {action.actionName}");
        //         // 重新开始重绑定
        //         currentRebindingOperation = InputSystem.onAnyButtonPress.CallOnce(OnAnyButtonPressed);
        //         return;
        //     }
        // }
        actionMap[actionName].currentKeyboardKey = newKey;
        isRebinding = false;
        currentRebindingOperation?.Dispose();
        currentRebindingOperation = null;
        
        // 启用输入缓冲
        EnableInputBuffer();
        
        SaveKeyBindings();
        UpdateButtonText(actionButtons[actionName], actionName);
        
        Debug.Log($"Bound {actionName} to {newKey}");
        waitingForInputPanel.SetActive(false);
        SetAllButtonsInteractable(true);
    }

    void UpdateButtonText(Button button, string actionName)
    {
        if (actionMap.ContainsKey(actionName))
        {
            var action = actionMap[actionName];
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = GetKeyDisplayName(action.currentKeyboardKey);
            }
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

    public void ResetToDefaults()
    {
        foreach (var action in inputActions)
        {
            action.currentKeyboardKey = action.defaultKeyboardKey;
        }
        
        SaveKeyBindings();
        
        // 更新所有按钮文本
        foreach (var kvp in actionButtons)
        {
            UpdateButtonText(kvp.Value, kvp.Key);
        }
        
        Debug.Log("All bindings reset to defaults");
    }

    #region 输入查询方法
    
    public bool GetButton(string actionName)
    {
        if (inputBufferEnabled) return false;
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return false;
        }
        
        var action = actionMap[actionName];
        return Keyboard.current[ConvertToKey(action.currentKeyboardKey)].isPressed;
    }
    
    public bool GetButtonDown(string actionName)
    {
        if (inputBufferEnabled) return false;
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return false;
        }
        
        var action = actionMap[actionName];
        return Keyboard.current[ConvertToKey(action.currentKeyboardKey)].wasPressedThisFrame;
    }
    
    public bool GetButtonUp(string actionName)
    {
        if (inputBufferEnabled) return false;
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return false;
        }
        
        var action = actionMap[actionName];
        return Keyboard.current[ConvertToKey(action.currentKeyboardKey)].wasReleasedThisFrame;
    }
    
    public Vector2 GetMovementAxis()
    {
        Vector2 input = Vector2.zero;
        
        if (GetButton("MoveRight")) input.x += 1;
        if (GetButton("MoveLeft")) input.x -= 1;
        if (GetButton("MoveUp")) input.y += 1;
        if (GetButton("MoveDown")) input.y -= 1;
        
        return input.normalized;
    }
    
    #endregion

    #region 键盘按键转换工具
    
    // 将传统 KeyCode 转换为新输入系统的 Key
    private Key ConvertToKey(KeyCode keyCode)
    {
        string keyName = keyCode.ToString();
        
        // 处理特殊按键的命名差异
        switch (keyCode)
        {
            case KeyCode.Alpha0: keyName = "Digit0"; break;
            case KeyCode.Alpha1: keyName = "Digit1"; break;
            case KeyCode.Alpha2: keyName = "Digit2"; break;
            case KeyCode.Alpha3: keyName = "Digit3"; break;
            case KeyCode.Alpha4: keyName = "Digit4"; break;
            case KeyCode.Alpha5: keyName = "Digit5"; break;
            case KeyCode.Alpha6: keyName = "Digit6"; break;
            case KeyCode.Alpha7: keyName = "Digit7"; break;
            case KeyCode.Alpha8: keyName = "Digit8"; break;
            case KeyCode.Alpha9: keyName = "Digit9"; break;
            case KeyCode.Mouse0: keyName = "LeftButton"; break;
            case KeyCode.Mouse1: keyName = "RightButton"; break;
            case KeyCode.Mouse2: keyName = "MiddleButton"; break;
        }
        
        if (System.Enum.TryParse<Key>(keyName, out Key key))
        {
            return key;
        }
        
        Debug.LogWarning($"Could not convert KeyCode {keyCode} to new Input System Key");
        return Key.None;
    }
    
    // 将新输入系统的 Key 转换为传统 KeyCode
    private KeyCode ConvertToKeyCode(Key key)
    {
        string keyName = key.ToString();
        
        // 处理特殊按键的命名差异
        switch (key)
        {
            case Key.Digit0: keyName = "Alpha0"; break;
            case Key.Digit1: keyName = "Alpha1"; break;
            case Key.Digit2: keyName = "Alpha2"; break;
            case Key.Digit3: keyName = "Alpha3"; break;
            case Key.Digit4: keyName = "Alpha4"; break;
            case Key.Digit5: keyName = "Alpha5"; break;
            case Key.Digit6: keyName = "Alpha6"; break;
            case Key.Digit7: keyName = "Alpha7"; break;
            case Key.Digit8: keyName = "Alpha8"; break;
            case Key.Digit9: keyName = "Alpha9"; break;
        }
        
        if (System.Enum.TryParse<KeyCode>(keyName, out KeyCode keyCode))
        {
            return keyCode;
        }
        
        Debug.LogWarning($"Could not convert Key {key} to KeyCode");
        return KeyCode.None;
    }
    
    #endregion

    #region 数据持久化
    
    public void SaveKeyBindings()
    {
        foreach (var action in inputActions)
        {
            PlayerPrefs.SetInt($"Key_{action.actionName}", (int)action.currentKeyboardKey);
        }
        PlayerPrefs.Save();
    }
    
    public void LoadKeyBindings()
    {
        foreach (var action in inputActions)
        {
            if (PlayerPrefs.HasKey($"Key_{action.actionName}"))
            {
                int keyValue = PlayerPrefs.GetInt($"Key_{action.actionName}");
                action.currentKeyboardKey = (KeyCode)keyValue;
            }
        }
    }
    
    #endregion

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
            "UIConfirm" => "确认",
            "UICancel" => "取消",
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
        // 清理资源
        currentRebindingOperation?.Dispose();
    }
}