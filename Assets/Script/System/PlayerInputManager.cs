using System.Collections;
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
    
    public UnityEvent OnButtonsCreated;
    // 添加按键变化事件
    public class KeyBindingChangedEvent : UnityEvent<string, KeyCode> { }
    public KeyBindingChangedEvent OnKeyBindingChanged = new KeyBindingChangedEvent();
    
    private bool inputBufferEnabled = false;
    private float inputBufferTime = 0.2f;
    private float lastRebindTime = 0f;

    [Header("Scroll View Settings")]
    public Transform bindingsContent; // ScrollView 的 Content 对象
    public GameObject bindingButtonPrefab; // 按键绑定按钮的预制体
    
    [Header("UI References")]
    public GameObject waitingForInputPanel;
    public TextMeshProUGUI waitingForInputText;
    
    [System.Serializable] public class InputAction
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
    
    private Dictionary<string, GameObject> actionButtonObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, Button> actionButtons = new Dictionary<string, Button>();
    private Dictionary<string, InputAction> actionMap = new Dictionary<string, InputAction>();
    public bool isRebinding = false;
    private string rebindingAction;
    private System.IDisposable currentRebindingOperation;

    void Awake()
    {
        Debug.unityLogger.Log("<color=#FF0000>-------PlayerInputManager instance-------</color>");
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
    
    void Start()
    {
        // 延迟一帧创建按钮，确保UI完全初始化
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        CreateBindingButtons();
        waitingForInputPanel.SetActive(false);
    }
    
    // 新增：获取当前按键显示名称的方法
    public string GetCurrentKeyDisplayName(string actionName)
    {
        if (actionMap.ContainsKey(actionName))
        {
            return GetKeyDisplayName(actionMap[actionName].currentKeyboardKey);
        }
        return "None";
    }
    
    // 新增：修改对应按键的方法
    public void ChangeKeyBinding(string actionName, KeyCode newKey)
    {
        if (actionMap.ContainsKey(actionName))
        {
            actionMap[actionName].currentKeyboardKey = newKey;
            SaveKeyBindings();
            OnKeyBindingChanged?.Invoke(actionName, newKey);
            
            // 更新按钮显示
            if (actionButtonObjects.ContainsKey(actionName))
            {
                UpdateButtonVisuals(actionButtonObjects[actionName], actionName);
            }
        }
    }
    
    // 修改：公开StartRebinding方法供其他脚本调用
    public void StartRebinding(string actionName)
    {
        if (isRebinding) return;
        
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return;
        }
        
        waitingForInputPanel.SetActive(true);
        waitingForInputText.text = $"等待输入...<size=70%> 为 {GetDisplayName(actionName)} 重新绑定</size>";
        
        SetAllButtonsInteractable(false);
        
        isRebinding = true;
        rebindingAction = actionName;
        
        currentRebindingOperation = InputSystem.onAnyButtonPress.CallOnce(OnAnyButtonPressed);
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
        
        // 如果正在重绑定，检测ESC键取消
        if (isRebinding && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelRebinding();
        }
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
            inputActions.Add(new InputAction("Attack_1", KeyCode.J));
            inputActions.Add(new InputAction("Skill_1", KeyCode.Q));
            inputActions.Add(new InputAction("Interact", KeyCode.E));
            inputActions.Add(new InputAction("Menu", KeyCode.I));
            inputActions.Add(new InputAction("UIMenu", KeyCode.P));
            inputActions.Add(new InputAction("UIUp", KeyCode.W));
            inputActions.Add(new InputAction("UIDown", KeyCode.S));
            inputActions.Add(new InputAction("UILeft", KeyCode.A));
            inputActions.Add(new InputAction("UIRight", KeyCode.D));
            inputActions.Add(new InputAction("UIConfirm", KeyCode.Return));
            inputActions.Add(new InputAction("UICancel", KeyCode.Escape));
        }
        
        foreach (var action in inputActions)
        {
            actionMap[action.actionName] = action;                      // 构建快速查找字典
            
            if (PlayerPrefs.HasKey($"Key_{action.actionName}"))         // 加载保存的按键设置
            {
                int keyValue = PlayerPrefs.GetInt($"Key_{action.actionName}");
                action.currentKeyboardKey = (KeyCode)keyValue;
            }
        }
    }
    
    void CreateBindingButtons()
    {
        // 清空现有按钮
        foreach (Transform child in bindingsContent)
        {
            Destroy(child.gameObject);
        }
        actionButtonObjects.Clear();
        actionButtons.Clear();

        // 创建分类和按钮
        CreateCategoryTitle("游戏行为");
        CreateButtonForAction("MoveUp");
        CreateButtonForAction("MoveDown");
        CreateButtonForAction("MoveLeft");
        CreateButtonForAction("MoveRight");
        CreateButtonForAction("Jump");
        
        CreateButtonForAction("Attack_1");
        CreateButtonForAction("Skill_1");
        CreateButtonForAction("Interact");
        CreateButtonForAction("Menu");
        
        CreateCategoryTitle("UI行为");
        CreateButtonForAction("UIMenu");
        CreateButtonForAction("UIUp");
        CreateButtonForAction("UIDown");
        CreateButtonForAction("UILeft");
        CreateButtonForAction("UIRight");
        CreateButtonForAction("UIConfirm");
        CreateButtonForAction("UICancel");
        
        CreateResetButton();
        
        RefreshLayout();
        OnButtonsCreated?.Invoke();
    }
    
    void CreateCategoryTitle(string title)
    {
        GameObject titleObj = new GameObject(title + "Title", typeof(RectTransform));
        titleObj.transform.SetParent(bindingsContent);
        
        LayoutElement layoutElem = titleObj.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = 40;
        
        RectTransform rect = titleObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 40);
        
        TextMeshProUGUI text = titleObj.AddComponent<TextMeshProUGUI>();
        text.text = title;
        text.fontSize = 22;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.2f, 0.2f, 0.2f);
        text.fontStyle = FontStyles.Bold;
    }

    public void CreateButtonForAction(string actionName)
    {
        if (!actionMap.ContainsKey(actionName)) return;
        if (bindingButtonPrefab == null) return;

        // 实例化按钮预制体
        GameObject buttonObj = Instantiate(bindingButtonPrefab, bindingsContent);
        
        // 设置Layout Element
        LayoutElement layoutElem = buttonObj.GetComponent<LayoutElement>();
        if (layoutElem == null)
        {
            layoutElem = buttonObj.AddComponent<LayoutElement>();
        }
        layoutElem.preferredHeight = 50;
        
        // 更新按钮显示
        UpdateButtonVisuals(buttonObj, actionName);
        
        // 找到内部的KeyButton并添加点击事件
        Transform keyButtonTransform = buttonObj.transform.Find("Button");
        if (keyButtonTransform != null)
        {
            Button keyButton = keyButtonTransform.GetComponent<Button>();
            if (keyButton != null)
            {
                keyButton.onClick.RemoveAllListeners();
                keyButton.onClick.AddListener(() => StartRebinding(actionName));
                
                // 存储按钮引用
                actionButtons[actionName] = keyButton;
            }
        }
        // 存储整个按钮对象的引用
        actionButtonObjects[actionName] = buttonObj;
    }

    void UpdateButtonVisuals(GameObject buttonObj, string actionName)
    {
        var action = actionMap[actionName];
        
        RectTransform rt = buttonObj.GetComponent<RectTransform>();         // 设置正确的缩放和锚点
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0, 1); // 左上锚点
        rt.anchorMax = new Vector2(1, 1); // 右上锚点
        rt.pivot = new Vector2(0.5f, 1); // 顶部中心轴心
        
        Transform actionNameText = buttonObj.transform.Find("Title");  // 更新动作名称文本
        RectTransform textRt = actionNameText.GetComponent<RectTransform>();
        // 设置文本的锚点 - 左侧垂直居中
        textRt.anchorMin = new Vector2(0, 0.5f);
        textRt.anchorMax = new Vector2(0, 0.5f);
        textRt.pivot = new Vector2(0, 0.5f);
        // 设置位置偏移
        textRt.anchoredPosition = new Vector2(10, 0);
        // 设置文本的宽度和高度
        textRt.sizeDelta = new Vector2(200, 30);

        TextMeshProUGUI actionText = actionNameText.GetComponent<TextMeshProUGUI>();
        actionText.text = GetDisplayName(action.actionName);
        
        
        Transform keyButtonTransform = buttonObj.transform.Find("Button");       // 更新按键名称文本

        RectTransform keyButtonRt = keyButtonTransform.GetComponent<RectTransform>();
        // 设置按键按钮的锚点 - 右侧垂直居中
        keyButtonRt.anchorMin = new Vector2(1, 0.5f);
        keyButtonRt.anchorMax = new Vector2(1, 0.5f);
        keyButtonRt.pivot = new Vector2(1, 0.5f);
        // 设置位置偏移
        keyButtonRt.anchoredPosition = new Vector2(-10, 0);
        // 设置按键按钮的宽度和高度
        keyButtonRt.sizeDelta = new Vector2(100, 40);

        Transform keyTextTransform = keyButtonTransform.Find("Text");
        RectTransform keyTextRt = keyTextTransform.GetComponent<RectTransform>();
        keyTextRt.anchorMin = Vector2.zero;
        keyTextRt.anchorMax = Vector2.one;
        keyTextRt.sizeDelta = Vector2.zero;
        keyTextRt.offsetMin = Vector2.zero;
        keyTextRt.offsetMax = Vector2.zero;
        TextMeshProUGUI buttonText = keyTextTransform.GetComponent<TextMeshProUGUI>();
        buttonText.text = GetKeyDisplayName(action.currentKeyboardKey);
    }
    
    void CreateResetButton()
    {
        GameObject buttonObj = Instantiate(bindingButtonPrefab, bindingsContent);
        
        LayoutElement layoutElem = buttonObj.GetComponent<LayoutElement>();
        if (layoutElem == null)
        {
            layoutElem = buttonObj.AddComponent<LayoutElement>();
        }
        layoutElem.preferredHeight = 50;
        
        // 修改重置按钮的显示
        Transform actionNameText = buttonObj.transform.Find("Title");
        if (actionNameText != null)
        {
            TextMeshProUGUI textComp = actionNameText.GetComponent<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = "重置所有设置为默认值";
                textComp.color = Color.red;
                textComp.alignment = TextAlignmentOptions.Center;
            }
        }
        
        // 隐藏按键按钮或修改其显示
        Transform keyButtonTransform = buttonObj.transform.Find("Button");
        if (keyButtonTransform != null)
        {
            // 修改按键按钮的文本
            Transform keyTextTransform = keyButtonTransform.Find("Text");
            if (keyTextTransform != null)
            {
                TextMeshProUGUI textComp = keyTextTransform.GetComponent<TextMeshProUGUI>();
                if (textComp != null)
                {
                    textComp.text = "重置";
                }
            }
            
            // 修改按键按钮的颜色
            Button keyButton = keyButtonTransform.GetComponent<Button>();
            if (keyButton != null)
            {
                ColorBlock colors = keyButton.colors;
                colors.normalColor = new Color(0.8f, 0.2f, 0.2f);
                colors.highlightedColor = new Color(1f, 0.3f, 0.3f);
                colors.pressedColor = new Color(0.6f, 0.1f, 0.1f);
                keyButton.colors = colors;
                
                keyButton.onClick.RemoveAllListeners();
                keyButton.onClick.AddListener(ResetToDefaults);
            }
        }
    }

    void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(bindingsContent as RectTransform);
        
        ScrollRect scrollRect = bindingsContent?.parent?.parent?.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1;
        }
    }
    
    // public void StartRebinding(string actionName)
    // {
    //     if (isRebinding) return;
    //     
    //     if (!actionMap.ContainsKey(actionName))
    //     {
    //         Debug.LogWarning($"Input action '{actionName}' not found!");
    //         return;
    //     }
    //     
    //     waitingForInputPanel.SetActive(true);
    //     waitingForInputText.text = $"wait for...<size=70%> {actionName} rebinding</size>";
    //     
    //     SetAllButtonsInteractable(false); // 禁用所有按钮避免重复点击
    //     
    //     isRebinding = true;
    //     rebindingAction = actionName;
    //     
    //     Debug.Log($"Press any key to bind for {actionName}... (Press Escape to cancel)");
    //     
    //     currentRebindingOperation = InputSystem.onAnyButtonPress.CallOnce(OnAnyButtonPressed);
    // }

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
        
        waitingForInputPanel.SetActive(false);
        SetAllButtonsInteractable(true);
    }

    void BindKey(string actionName, KeyCode newKey)
    {
        if (!actionMap.ContainsKey(actionName)) return;
        
        actionMap[actionName].currentKeyboardKey = newKey;
        isRebinding = false;
        currentRebindingOperation?.Dispose();
        currentRebindingOperation = null;
        
        EnableInputBuffer();
        SaveKeyBindings();
        
        OnKeyBindingChanged?.Invoke(actionName, newKey);//更新提示按钮的文本
        // 更新按钮显示
        if (actionButtonObjects.ContainsKey(actionName))
        {
            UpdateButtonVisuals(actionButtonObjects[actionName], actionName);
        }
        
        waitingForInputPanel.SetActive(false);
        SetAllButtonsInteractable(true);
    }

    void UpdateButtonText(string actionName)
    {
        if (actionMap.ContainsKey(actionName) && actionButtons.ContainsKey(actionName))
        {
            var action = actionMap[actionName];
            Button button = actionButtons[actionName];
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{GetDisplayName(action.actionName)}: {GetKeyDisplayName(action.currentKeyboardKey)}";
            }
        }
    }

    void SetAllButtonsInteractable(bool interactable)
    {
        foreach (var button in actionButtons.Values)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }

    public void ResetToDefaults()
    {
        foreach (var action in inputActions)
        {
            action.currentKeyboardKey = action.defaultKeyboardKey;
            UpdateButtonText(action.actionName);
            OnKeyBindingChanged?.Invoke(action.actionName, action.defaultKeyboardKey);
        }
        
        SaveKeyBindings();
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
        
        return KeyCode.None;
    }
    
    #endregion
    
    public void SaveKeyBindings()
    {
        foreach (var action in inputActions)
        {
            PlayerPrefs.SetInt($"Key_{action.actionName}", (int)action.currentKeyboardKey);
        }
        PlayerPrefs.Save();
    }

    #region 显示名称转换
    public string GetDisplayName(string actionName)
    {
        return actionName switch
        {
            "MoveUp" => "向上移动",
            "MoveDown" => "向下移动", 
            "MoveLeft" => "向左移动",
            "MoveRight" => "向右移动",
            "Jump" => "跳跃",
            "Attack_1" => "攻击",
            "Skill_1" => "技能",
            "Interact" => "交互",
            "Menu" => "游戏菜单",
            "UIMenu" => "UI菜单",
            "UIUp" => "UI向上",
            "UIDown" => "UI向下",
            "UILeft" => "UI向左",
            "UIRight" => "UI向右",
            "UIConfirm" => "UI确认",
            "UICancel" => "UI取消",
            _ => actionName
        };
    }

    public string GetKeyDisplayName(KeyCode keyCode)
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
            KeyCode.Tab => "Tab",
            KeyCode.CapsLock => "大写锁定",
            KeyCode.Backspace => "退格",
            KeyCode.Insert => "Insert",
            KeyCode.Delete => "Delete",
            KeyCode.Home => "Home",
            KeyCode.End => "End",
            KeyCode.PageUp => "PageUp",
            KeyCode.PageDown => "PageDown",
            KeyCode.Keypad0 => "小键盘0",
            KeyCode.Keypad1 => "小键盘1",
            KeyCode.Keypad2 => "小键盘2",
            KeyCode.Keypad3 => "小键盘3",
            KeyCode.Keypad4 => "小键盘4",
            KeyCode.Keypad5 => "小键盘5",
            KeyCode.Keypad6 => "小键盘6",
            KeyCode.Keypad7 => "小键盘7",
            KeyCode.Keypad8 => "小键盘8",
            KeyCode.Keypad9 => "小键盘9",
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