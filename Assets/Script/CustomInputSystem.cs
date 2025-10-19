using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomInputSystem : MonoBehaviour
{
    public static CustomInputSystem instance { get; private set; }
    
    [System.Serializable] public class InputAction
    {
        public string actionName;
        public KeyCode defaultKeyboardKey;
        public string defaultGamepadButton;
        public KeyCode currentKeyboardKey;
        public string currentGamepadButton;
        
        public InputAction(string name, KeyCode keyboardKey, string gamepadButton)
        {
            actionName = name;
            defaultKeyboardKey = keyboardKey;
            defaultGamepadButton = gamepadButton;
            currentKeyboardKey = keyboardKey;
            currentGamepadButton = gamepadButton;
        }
    }
    
    public enum InputDevice { Keyboard, Gamepad }
    
    [Header("Settings")]
    public InputDevice currentDevice = InputDevice.Keyboard;
    public float gamepadCheckInterval = 0.5f;
    public float axisDeadZone = 0.2f;
    
    [Header("Input Actions")]
    public List<InputAction> inputActions = new List<InputAction>();
    
    // 事件
    public event Action<InputDevice> OnDeviceChanged;
    public event Action<string> OnActionRebound;
    
    private Dictionary<string, InputAction> actionMap = new Dictionary<string, InputAction>();
    private float lastGamepadCheckTime;
    public bool isRebinding = false;
    private string rebindingAction;
    private bool rebindingForKeyboard;
    private Coroutine rebindingCoroutine;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeInputSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        DetectInputDevice();
        if (isRebinding)
        {
            ProcessRebinding();
        }
    }
    
    void InitializeInputSystem()
    {
        // 创建默认输入映射
        if (inputActions.Count == 0)
        {
            inputActions.Add(new InputAction("MoveUp", KeyCode.W, "DPadUp"));
            inputActions.Add(new InputAction("MoveDown", KeyCode.S, "DPadDown"));
            inputActions.Add(new InputAction("MoveLeft", KeyCode.A, "DPadLeft"));
            inputActions.Add(new InputAction("MoveRight", KeyCode.D, "DPadRight"));
            inputActions.Add(new InputAction("Jump", KeyCode.Space, "ButtonSouth"));
            inputActions.Add(new InputAction("Attack", KeyCode.Mouse0, "ButtonWest"));
            inputActions.Add(new InputAction("Interact", KeyCode.E, "ButtonEast"));
            inputActions.Add(new InputAction("Menu", KeyCode.Escape, "ButtonStart"));
        }
        // 构建快速查找字典
        foreach (var action in inputActions)
        {
            actionMap[action.actionName] = action;
        }
        // 加载保存的按键设置
        LoadKeyBindings();
    }
    
    void DetectInputDevice()
    {
        if (Time.time - lastGamepadCheckTime < gamepadCheckInterval) return;
        
        lastGamepadCheckTime = Time.time;
        
        InputDevice detectedDevice = currentDevice;
        
        // 检查手柄输入
        if (IsGamepadInput())
        {
            detectedDevice = InputDevice.Gamepad;
        }
        // 检查键盘输入
        else if (IsKeyboardInput())
        {
            detectedDevice = InputDevice.Keyboard;
        }
        
        // 设备切换
        if (detectedDevice != currentDevice)
        {
            currentDevice = detectedDevice;
            OnDeviceChanged?.Invoke(currentDevice);
            Debug.Log($"Input device switched to: {currentDevice}");
        }
    }
    
    bool IsGamepadInput()
    {
        // 检查手柄按钮
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + i)))
                return true;
        }
        
        // 检查手柄摇杆
        string[] axes = { "Horizontal", "Vertical", "Horizontal2", "Vertical2" };
        foreach (string axis in axes)
        {
            if (Mathf.Abs(Input.GetAxis(axis)) > axisDeadZone)
                return true;
        }
        
        // 检查手柄触发器
        if (Mathf.Abs(Input.GetAxis("Triggers")) > axisDeadZone)
            return true;
        
        return false;
    }
    
    bool IsKeyboardInput()
    {
        // 检查键盘按键（排除鼠标，因为鼠标移动太敏感）
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z && Input.GetKey(keyCode))
                return true;
                
            if ((keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9) && Input.GetKey(keyCode))
                return true;
                
            if ((keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9) && Input.GetKey(keyCode))
                return true;
                
            if ((keyCode >= KeyCode.UpArrow && keyCode <= KeyCode.DownArrow) && Input.GetKey(keyCode))
                return true;
        }
        
        return false;
    }
    
    #region 输入查询方法
    
    public bool GetButton(string actionName)
    {
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return false;
        }
        var action = actionMap[actionName];
        
        if (currentDevice == InputDevice.Keyboard)
        {
            // Debug.Log("Keyboard Input: " + actionName );
            // if (Input.GetKey(action.currentKeyboardKey))
            // {
            //     Debug.Log("xxxx: " );
            // }
            return Input.GetKey(action.currentKeyboardKey);
        }
        else
        {
            return Input.GetButton(action.currentGamepadButton);
        }
    }
    
    public bool GetButtonDown(string actionName)
    {
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return false;
        }
        var action = actionMap[actionName];
        
        if (currentDevice == InputDevice.Keyboard)
        {
            return Input.GetKeyDown(action.currentKeyboardKey);
        }
        else
        {
            return Input.GetButtonDown(action.currentGamepadButton);
        }
    }
    
    public bool GetButtonUp(string actionName)
    {
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Input action '{actionName}' not found!");
            return false;
        }
        
        var action = actionMap[actionName];
        
        if (currentDevice == InputDevice.Keyboard)
        {
            return Input.GetKeyUp(action.currentKeyboardKey);
        }
        else
        {
            return Input.GetButtonUp(action.currentGamepadButton);
        }
    }
    
    public float GetAxis(string axisName)
    {
        // 处理摇杆输入
        return Input.GetAxis(axisName);
    }
    
    public Vector2 GetMovementAxis()
    {
        if (currentDevice == InputDevice.Keyboard)
        {
            float x = (GetButton("MoveRight") ? 1 : 0) - (GetButton("MoveLeft") ? 1 : 0);
            float y = (GetButton("MoveUp") ? 1 : 0) - (GetButton("MoveDown") ? 1 : 0);
            return new Vector2(x, y).normalized;
        }
        else
        {
            return new Vector2(GetAxis("Horizontal"), GetAxis("Vertical"));
        }
    }
    
    #endregion
    
    #region 按键重绑定
    
    public void StartRebinding(string actionName, bool forKeyboard)
    {
        if (isRebinding) return;
        
        if (!actionMap.ContainsKey(actionName))
        {
            Debug.LogWarning($"Cannot rebind - action '{actionName}' not found!");
            return;
        }
        
        isRebinding = true;
        rebindingAction = actionName;
        rebindingForKeyboard = forKeyboard;
        
        Debug.Log($"Rebinding {actionName} for {(forKeyboard ? "Keyboard" : "Gamepad")}. Press any key...");
        
        if (rebindingCoroutine != null)
            StopCoroutine(rebindingCoroutine);
            
        rebindingCoroutine = StartCoroutine(RebindingTimeout(5f));
    }
    
    public void CancelRebinding()
    {
        if (!isRebinding) return;
        
        isRebinding = false;
        rebindingAction = null;
        
        if (rebindingCoroutine != null)
        {
            StopCoroutine(rebindingCoroutine);
            rebindingCoroutine = null;
        }
        
        Debug.Log("Rebinding cancelled");
    }
    
    void ProcessRebinding()
    {
        if (!isRebinding) return;
        
        if (rebindingForKeyboard)
        {
            // 键盘重绑定
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    // 跳过不应该绑定的键
                    if (keyCode == KeyCode.None || keyCode == KeyCode.Escape)
                        continue;
                    
                    BindKey(rebindingAction, keyCode);
                    return;
                }
            }
        }
        else
        {
            // 手柄重绑定
            for (int i = 0; i < 20; i++)
            {
                KeyCode joystickButton = (KeyCode)((int)KeyCode.JoystickButton0 + i);
                if (Input.GetKeyDown(joystickButton))
                {
                    string buttonName = $"JoystickButton{i}";
                    BindGamepadButton(rebindingAction, buttonName);
                    return;
                }
            }
            
            // 检查摇杆输入（作为备选手柄绑定方式）
            CheckAxisForRebinding();
        }
    }
    
    void CheckAxisForRebinding()
    {
        // 这里可以添加对摇杆和触发器输入的检测
        // 简化实现，实际项目中需要更详细的手柄输入检测
    }
    
    void BindKey(string actionName, KeyCode newKey)
    {
        if (!actionMap.ContainsKey(actionName)) return;
        
        // 检查按键是否已被使用
        foreach (var action in inputActions)
        {
            if (action.actionName != actionName && action.currentKeyboardKey == newKey)
            {
                Debug.LogWarning($"Key {newKey} is already bound to {action.actionName}");
                return;
            }
        }
        
        actionMap[actionName].currentKeyboardKey = newKey;
        isRebinding = false;
        
        SaveKeyBindings();
        OnActionRebound?.Invoke(actionName);
        
        Debug.Log($"Bound {actionName} to {newKey}");
    }
    
    void BindGamepadButton(string actionName, string newButton)
    {
        if (!actionMap.ContainsKey(actionName)) return;
        
        actionMap[actionName].currentGamepadButton = newButton;
        isRebinding = false;
        
        SaveKeyBindings();
        OnActionRebound?.Invoke(actionName);
        
        Debug.Log($"Bound {actionName} to {newButton}");
    }
    
    IEnumerator RebindingTimeout(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        
        if (isRebinding)
        {
            Debug.Log("Rebinding timed out");
            CancelRebinding();
        }
    }
    
    public void ResetToDefaults()
    {
        foreach (var action in inputActions)
        {
            action.currentKeyboardKey = action.defaultKeyboardKey;
            action.currentGamepadButton = action.defaultGamepadButton;
        }
        
        SaveKeyBindings();
        Debug.Log("All bindings reset to defaults");
    }
    
    #endregion
    
    #region 数据持久化
    
    public void SaveKeyBindings()
    {
        foreach (var action in inputActions)
        {
            PlayerPrefs.SetInt($"Key_{action.actionName}", (int)action.currentKeyboardKey);
            PlayerPrefs.SetString($"Button_{action.actionName}", action.currentGamepadButton);
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
            
            if (PlayerPrefs.HasKey($"Button_{action.actionName}"))
            {
                action.currentGamepadButton = PlayerPrefs.GetString($"Button_{action.actionName}");
            }
        }
    }
    
    #endregion
}